using SFML.Graphics;
using SFML.System;
using System;

namespace SFCORE
{
    public interface IWindowUtil
    {
        FloatRect GetFractionalRect(FloatRect floatRect);
        Vector2f GetPixelSize(Vector2f fractionalWindowSize);
        Vector2u WindowSize { get; }
    }

    class WindowUtilUtil : IWindowUtil
    {
        private readonly Func<Vector2u> _getWindowSize;

        public WindowUtilUtil(Func<Vector2u> getWindowSize)
        {
            _getWindowSize = getWindowSize;
        }

        public FloatRect GetFractionalRect(FloatRect floatRect)
        {
            var windowSize = _getWindowSize();
            return new FloatRect(floatRect.Left / windowSize.X, floatRect.Top / windowSize.Y, floatRect.Width / windowSize.X, floatRect.Height / windowSize.Y);
        }

        public Vector2f GetPixelSize(Vector2f fractionalWindowSize)
        {
            var ws = _getWindowSize();
            return new Vector2f(fractionalWindowSize.X * ws.X, fractionalWindowSize.Y * ws.Y);
        }

        public Vector2u WindowSize => _getWindowSize();
    }
}