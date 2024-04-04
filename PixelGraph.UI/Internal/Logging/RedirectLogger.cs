using Microsoft.Extensions.Logging;

namespace PixelGraph.UI.Internal.Logging;

internal class RedirectLogger(ILogReceiver receiver) : RedirectLogger<object>(receiver);

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

public class LogEventArgs(LogLevel level, string message) : EventArgs
{
    public LogLevel Level {get; set;} = level;
    public string Message {get; set;} = message;
}
