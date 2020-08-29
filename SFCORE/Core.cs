using SFML.Graphics;
using System;

namespace SFCORE
{
    public static class Core
    {
        private static bool _initialized;
        public static ITimeInfo TimeInfo { get; private set; }
        public static IInput Input { get; private set; }
        public static ILogger Logger => _getLogger();
        public static ITextInfo Text { get; private set; }
        public static RenderWindow Window { get; internal set; }

        public static void Initialize(RenderWindow window, ITimeInfo timeInfo, IInput input, IWindowUtil windowUtil, Func<ILogger> getLogger, ITextInfo text)
        {
            Window = window;
            if (_initialized)
                return;
            _initialized = true;
            TimeInfo = timeInfo;
            Input = input;
            WindowUtil = windowUtil;
            _getLogger = getLogger;
            Text = text;
        }

        public static IWindowUtil WindowUtil;
        private static Func<ILogger> _getLogger;
    }
}