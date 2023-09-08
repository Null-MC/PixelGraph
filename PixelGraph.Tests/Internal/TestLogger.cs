using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal;

internal class TestLogger : TestLogger<object>
{
    public TestLogger(ITestOutputHelper output) : base(output) {}
}

internal class TestLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper output;


    public TestLogger(ITestOutputHelper output)
    {
        this.output = output;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        output.WriteLine($"LOG: {message}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}