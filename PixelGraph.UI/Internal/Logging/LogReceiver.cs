using System;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace PixelGraph.UI.Internal.Logging;

internal interface ILogReceiver
{
    void Log(LogLevel level, string message);
}

//internal class LogReceiver : ILogReceiver
//{
//    public event EventHandler<LogEventArgs> LogMessage;


//    public void Log(LogLevel level, string message)
//    {
//        LogMessage?.Invoke(this, new LogEventArgs(level, message));
//    }
//}

internal class SerilogReceiver : ILogReceiver, Serilog.ILogger
{
    public event EventHandler<LogEventArgs> LogMessage;


    public void Log(LogLevel level, string message)
    {
        OnLog(level, message);

        var _level = level switch {
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.Trace => LogEventLevel.Verbose,
            _ => throw new ArgumentOutOfRangeException(),
        };

        Serilog.Log.Write(_level, message);
    }

    public void Write(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        var level = logEvent.Level switch {
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            LogEventLevel.Verbose => LogLevel.Trace,
            _ => throw new ArgumentOutOfRangeException(),
        };

        OnLog(level, message);
        Serilog.Log.Write(logEvent);
    }

    protected virtual void OnLog(LogLevel level, string message)
    {
        LogMessage?.Invoke(this, new LogEventArgs(level, message));
    }
}