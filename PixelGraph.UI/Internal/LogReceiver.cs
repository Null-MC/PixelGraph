using Microsoft.Extensions.Logging;
using System;

namespace PixelGraph.UI.Internal
{
    internal interface ILogReceiver
    {
        void Log(LogLevel level, string message);
    }

    internal class LogReceiver : ILogReceiver
    {
        public event EventHandler<LogEventArgs> LogMessage;


        public void Log(LogLevel level, string message)
        {
            LogMessage?.Invoke(this, new LogEventArgs(level, message));
        }
    }
}
