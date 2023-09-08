using PixelGraph.Common.IO;
using Serilog;
using Serilog.Core;
using System.IO;

namespace PixelGraph.Common;

public static class LocalLogFile
{
    public static string LogPath {get;}
    public static Logger FileLogger {get;}


    static LocalLogFile()
    {
        LogPath = Path.Join(AppDataHelper.AppDataPath, "logs");
        var logFile = Path.Join(LogPath, "log_.txt");

        FileLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 3,
                //buffered: true,
                shared: true)
            .CreateLogger();
    }
}