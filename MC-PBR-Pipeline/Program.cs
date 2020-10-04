using McPbrPipeline.CommandLine;
using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace McPbrPipeline
{
    internal class Program
    {
        private static readonly AppLifetime lifetime;


        static Program()
        {
            lifetime = new AppLifetime();
        }

        public static async Task<int> Main(string[] args)
        {
            Console.CancelKeyPress += Console_OnCancelKeyPress;

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            try {
                var services = new ServiceCollection();
                ConfigureServices(services);

                await using var provider = services.BuildServiceProvider();
                var commandLine = provider.GetRequiredService<IAppCommandLine>();

                return await commandLine.RunAsync(args);
            }
            catch (Exception error) {
                Log.Logger.Fatal(error, "An unhandled exception has occurred!");
                return 1;
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => {
                builder.AddSerilog();
            });

            services.AddSingleton<IAppLifetime>(lifetime);
            services.AddSingleton<IAppCommandLine, AppCommandLine>();
            //services.AddSingleton<IFileLoader, FileLoader>();
            services.AddSingleton<IPublisher, Publisher>();
        }

        private static void Console_OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ConsoleEx.WriteLine("Cancelling...", ConsoleColor.Yellow);
            lifetime.Cancel();
            e.Cancel = true;
        }
    }
}
