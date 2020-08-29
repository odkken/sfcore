using SFML.Graphics;

namespace SFCORE.Terminal
{
    public enum Tag
    {
        Input,
        Response,
        Error,
        Warning,
        Debug,
        SuperLowDebug
    }
    public interface IWrappedTextRenderer : Drawable
    {
        void AddLine(string line, Tag tag);
        void ScrollUp();
        void ScrollDown();
    }
}