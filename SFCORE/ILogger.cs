namespace SFCORE
{
    public enum Category
    {
        SuperLowDebug,
        Debug,
        Warning,
        Error
    }
    public interface ILogger
    {
        void Log(string msg, Category category = Category.Debug);
    }
}