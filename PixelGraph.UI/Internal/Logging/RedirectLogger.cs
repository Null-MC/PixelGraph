using Microsoft.Extensions.Logging;

namespace PixelGraph.UI.Internal.Logging;

internal class RedirectLogger : RedirectLogger<object>
{
    public RedirectLogger(ILogReceiver receiver) : base(receiver) {}
}

internal class RedirectLogger<T> : ILogger<T>
{
    private readonly ILogReceiver receiver;

    public bool IsEnabled(LogLevel logLevel) => true;


    protected RedirectLogger(ILogReceiver receiver)
    {
        this.receiver = receiver;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        receiver.Log(logLevel, message);
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }
}

internal class LogEventArgs : EventArgs
{
    public LogLevel Level {get; set;}
    public string Message {get; set;}

    public LogEventArgs(LogLevel level, string message)
    {
        Level = level;
        Message = message;
    }
}