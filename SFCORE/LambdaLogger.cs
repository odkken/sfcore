using System;

namespace SFCORE
{
    public class LambdaLogger : ILogger
    {
        private readonly Action<string, Category> _messageHandler;

        public LambdaLogger(Action<string, Category> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void Log(string msg, Category category = Category.Debug)
        {
            _messageHandler(msg, category);
        }
    }
}