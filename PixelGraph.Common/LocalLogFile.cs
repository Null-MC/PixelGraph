using PixelGraph.Common.IO;
using Serilog;
using Serilog.Core;
using System.IO;

namespace PixelGraph.Common
{
    public static class LocalLogFile
    {
        public static Logger FileLogger {get;}


        static LocalLogFile()
        {
            var logPath = Path.Join(AppDataHelper.AppDataPath, "logs");
            var logFile = Path.Join(logPath, "log_.txt");

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
}
