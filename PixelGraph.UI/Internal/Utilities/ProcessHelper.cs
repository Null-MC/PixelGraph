using System.Diagnostics;

namespace PixelGraph.UI.Internal.Utilities;

internal static class ProcessHelper
{
    public static void Start(string filename)
    {
        var info = new ProcessStartInfo {
            FileName = filename,
            UseShellExecute = true,
        };

        using var process = Process.Start(info);
        //process?.WaitForInputIdle(3_000);
    }
}