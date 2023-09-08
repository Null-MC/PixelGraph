using System;
using System.Threading;

namespace PixelGraph.CLI;

internal interface IAppLifetime
{
    CancellationToken Token {get;}

    void Cancel();
}

internal class AppLifetime : IAppLifetime, IDisposable
{
    private readonly CancellationTokenSource tokenSource;

    public CancellationToken Token => tokenSource.Token;


    public AppLifetime()
    {
        tokenSource = new CancellationTokenSource();
    }

    public void Cancel() => tokenSource.Cancel();

    public void Dispose()
    {
        tokenSource?.Dispose();
    }
}