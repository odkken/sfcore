using System;
using System.Collections.Generic;
using System.Linq;

namespace SFCORE.Terminal
{
    public class CommandRunner : ICommandRunner
    {
        private readonly IEnumerable<CommandData> _commands;

        public CommandRunner(IEnumerable<CommandData> commands)
        {
            _commands = commands;
        }

        public List<string> RunCommand(string command)
        {
            if(string.IsNullOrWhiteSpace(command))
                return new List<string>();
            var split = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim());
            var c = _commands.SingleOrDefault(a => a.Name == split.First());
            return c == null ? new List<string> { $"{split.First()}: command not found" } : c.Invoke(split.Skip(1).ToArray());
        }
    }
}