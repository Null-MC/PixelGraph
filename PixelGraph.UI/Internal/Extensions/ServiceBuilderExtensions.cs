using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;

namespace PixelGraph.UI.Internal.Extensions
{
    internal static class ServiceBuilderExtensions
    {
        public static LogReceiver AddLoggingRedirect(this IServiceBuilder serviceBuilder)
        {
            var logReceiver = new LogReceiver();
            //logReceiver.LogMessage += logEvent;

            serviceBuilder.Services.AddSingleton<ILogReceiver>(logReceiver);
            serviceBuilder.Services.AddSingleton(typeof(ILogger<>), typeof(RedirectLogger<>));
            serviceBuilder.Services.AddSingleton<ILogger, RedirectLogger>();

            return logReceiver;
        }
    }
}
