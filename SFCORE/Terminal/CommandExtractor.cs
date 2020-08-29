using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SFCORE.Terminal
{
    public class CommandExtractor
    {
        private readonly ILogger _logger;

        public CommandExtractor(ILogger logger)
        {
            _logger = logger;
        }

        public List<CommandData> GetAllStaticCommands(Assembly ass)
        {
            var commands = new List<CommandData>();
            foreach (var type in ass.GetTypes())
            {
                var typesCommands = type.GetMethods().Where(a => a.CustomAttributes.Any(b => b.AttributeType == typeof(CommandAttribute)));
                foreach (var method in typesCommands)
                {
                    if (!method.IsStatic)
                    {
                        _logger.Log($"Can't register command {method.Name} because it isn't static.", Category.Error);
                        continue;
                    }
                    commands.Add(new CommandData
                    {
                        Name = method.Name,
                        Invoke = strings =>
                        {
                            var parms = method.GetParameters();
                            if (parms.Length != strings.Length)
                            {
                                _logger.Log($"{method.Name} argument mismatch.  Expected <{string.Join(",", parms.Select(a => a.ParameterType))}>", Category.Error);
                                return new List<string>();
                            }
                            var typedArgs = new List<object>();
                            var parameterErrors = new List<string>();
                            for (int i = 0; i < parms.Length; i++)
                            {
                                try
                                {
                                    typedArgs.Add(Convert.ChangeType(strings[i], parms[i].ParameterType));
                                }
                                catch (Exception e)
                                {
                                    parameterErrors.Add($"Error parsing {strings[i]} to type {parms[i].ParameterType}:");
                                    parameterErrors.Add(e.Message);
                                }
                            }
                            if (parameterErrors.Any())
                            {
                                parameterErrors.ForEach(a => _logger.Log(a, Category.Error));
                                return new List<string>();
                            }

                            object returnVal = null;
                            try
                            {
                                returnVal = method.Invoke(null, typedArgs.ToArray());
                            }
                            catch (Exception e)
                            {
                                _logger.Log($"Error executing command {method.Name}:\n{e}", Category.Error);
                                return new List<string>();
                            }
                            if (returnVal is IEnumerable<object>)
                                return (returnVal as IEnumerable<object>).Select(a => a.ToString()).ToList();
                            return new List<string> { returnVal?.ToString() };
                        }
                    });
                }
            }
            _logger.Log($"Extracted comands: {string.Join(",", commands.Select(a => a.Name))}");
            return commands;
        }
    }
}