using Microsoft.Extensions.DependencyInjection;
using PixelGraph.CLI.CommandLine;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading.Tasks;

namespace PixelGraph.CLI
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
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    LogEventLevel.Information, 
                    "{Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code,
                    applyThemeToRedirectedOutput: true)
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
            services.AddTransient<IServiceBuilder, ServiceBuilder>();

            //services.AddSingleton<ImportCommand>();
            //services.AddSingleton<ConvertCommand>();
            //services.AddSingleton<GenerateCommand>();
            //services.AddSingleton<GenerateNormalCommand>();
            //services.AddSingleton<GenerateOcclusionCommand>();
            services.AddSingleton<PublishCommand>();
            services.AddTransient<PublishCommand.Executor>();
        }

        private static void Console_OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ConsoleEx.WriteLine("Cancelling...", ConsoleColor.Yellow);
            lifetime.Cancel();
            e.Cancel = true;
        }
    }
}
