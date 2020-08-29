using SFCORE.Terminal;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace SFCORE
{
    class Program
    {
        private static Terminal.Terminal _terminal;
        private static List<CommandData> _commands;
        private static ILogger _logger;
        private static IInput _terminalInput;
        private static BlockableInput _editInput;
        private static BlockableInput _gameInput;

        [Command]
        public static List<string> help()
        {
            return _commands.Select(a => a.Name).ToList();
        }

        [Command]
        public static void SetLogLevel(string category)
        {
            _logCategory = Enum.Parse<Category>(category, true);
        }

        private static Category _logCategory = Category.Debug;
        static void Main(string[] args)
        {
            RunCode();
        }

        class Controllable
        {
            private IInput _input;

            public Controllable(IInput input)
            {
                _input = input;
            }

            public void Update(float dt)
            {
                var ds = new Vector2();
                if (_input.IsKeyDown(Keyboard.Key.W))
                    ds.Y -= 1;
                if (_input.IsKeyDown(Keyboard.Key.A))
                    ds.X -= 1;
                if (_input.IsKeyDown(Keyboard.Key.S))
                    ds.Y += 1;
                if (_input.IsKeyDown(Keyboard.Key.D))
                    ds.X += 1;
                Move(ds * dt * 100);
            }

            public Vector2 Position { get; set; }
            public void Move(Vector2 delta)
            {
                Position += delta;
            }
        }

        public class Character : Drawable
        {
            private readonly Func<Vector2, Drawable> _getDrawable;
            private readonly Func<Vector2> _getPos;

            public Character(Func<Vector2, Drawable> getDrawable, Func<Vector2> getPos)
            {
                _getDrawable = getDrawable;
                _getPos = getPos;
            }


            public void Draw(RenderTarget target, RenderStates states)
            {
                _getDrawable(_getPos()).Draw(target, states);
            }
        }
        static void InitTerminal()
        {
            CommandRunner runner = null;
            _terminal = new Terminal.Terminal(Core.Window, Core.Text.DefaultFont, _terminalInput, () => runner, s => string.IsNullOrWhiteSpace(s) ? new List<string>() : _commands.Where(a => a.Name.ToLower().Contains(s.ToLower())).Select(a => a.Name).OrderBy(a => a.Length).ToList());
            var commandExtractor = new CommandExtractor(_logger);
            _commands = commandExtractor.GetAllStaticCommands(Assembly.GetExecutingAssembly());
            runner = new CommandRunner(_commands);
            sc(.5f, .5f, .2f, .5f);
        }

        [Command]
        public static float GetTime()
        {
            return Core.TimeInfo.CurrentTime;
        }
        [Command]
        public static float dt()
        {
            return Core.TimeInfo.CurrentDt;
        }

        [Command]
        public static List<string> PrintStuff(int thing1, string name)
        {
            return new List<string>
            {
                $"Hi {name}",
                $"I am {thing1}"
            };
        }

        [Command]
        public static void sc(float r, float g, float b, float a)
        {
            _terminal.SetHighlightColor(new Color((byte)(255 * r), (byte)(255 * g), (byte)(255 * b), (byte)(255 * a)));
        }

        static void RunCode()
        {
            var window = new RenderWindow(new VideoMode(1080, 1080), "Mirror");
            window.SetVerticalSyncEnabled(false);
            window.SetActive();
            window.Closed += (sender, eventArgs) => window.Close();

            var timeInfo = new TimeInfo();

            var globalInput = new WindowWrapperInput(window);

            globalInput.KeyPressed += args =>
            {
                if (args.Code == Keyboard.Key.Escape)
                    window.Close();
            };

            _terminalInput = new BlockableInput(globalInput);
            _editInput = new BlockableInput(_terminalInput);
            _gameInput = new BlockableInput(_editInput);

            _logger = new LambdaLogger((a, b) =>
            {
                if (b >= _logCategory) _terminal.LogMessage(a, b);
            });

            Core.Initialize(window, timeInfo, _gameInput, new WindowUtilUtil(() => window.Size), () => _logger, new TextInfo() { DefaultFont = new Font(@"D:\git\SFCORE\SFCORE\SFCORE\Inconsolata-Regular.ttf") });
            InitTerminal();



            var playerShape = new CircleShape(10) { FillColor = Color.Blue };
            var playercontrollable = new Controllable(_gameInput);
            var player = new Character(a => { playerShape.Position = a.ToSFVec(); return playerShape; }, () => playercontrollable.Position);





            while (window.IsOpen)
            {
                timeInfo.Tick();
                window.DispatchEvents();
                globalInput.Update(timeInfo.CurrentDt);

                window.Clear();
                window.DispatchEvents();
                globalInput.Update(timeInfo.CurrentDt);
                playercontrollable.Update(timeInfo.CurrentDt);
                window.Clear(Color.Black);
                window.Draw(_terminal);
                window.Draw(player);
                window.Display();
            }
        }
    }
}
