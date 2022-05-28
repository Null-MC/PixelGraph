using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.UI.Internal.Logging;
using Serilog;

namespace PixelGraph.UI.Internal.Extensions
{
    internal static class ServiceBuilderExtensions
    {
        //public static LogReceiver AddLoggingRedirect(this IServiceBuilder serviceBuilder)
        //{
        //    var logReceiver = new LogReceiver();
        //    //logReceiver.LogMessage += logEvent;

        //    serviceBuilder.Services.AddSingleton<ILogReceiver>(logReceiver);
        //    serviceBuilder.Services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(RedirectLogger<>));
        //    serviceBuilder.Services.AddSingleton<Microsoft.Extensions.Logging.ILogger, RedirectLogger>();

        //    return logReceiver;
        //}

        public static SerilogReceiver AddSerilogRedirect(this IServiceBuilder serviceBuilder)
        {
            var logReceiver = new SerilogReceiver();
            serviceBuilder.Services.AddSingleton<ILogReceiver>(logReceiver);
            serviceBuilder.Services.AddLogging(builder => builder.AddSerilog(logReceiver));
            return logReceiver;
        }
    }
}
