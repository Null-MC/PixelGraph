﻿using System.Diagnostics;

namespace PixelGraph.UI.Internal.Extensions;

internal static class ProcessExtensions
{
    /// <summary>
    /// Waits asynchronously for the process to exit.
    /// </summary>
    /// <param name="process">The process to wait for cancellation.</param>
    /// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
    /// immediately as canceled.</param>
    /// <returns>A Task representing waiting for the process to end.</returns>
    public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        if (process.HasExited) return Task.CompletedTask;

        var tcs = new TaskCompletionSource<object?>();
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => tcs.TrySetResult(null);

        cancellationToken.Register(() => tcs.SetCanceled(cancellationToken));

        return process.HasExited ? Task.CompletedTask : tcs.Task;
    }
}