using McPbrPipeline.Internal.Publishing;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace McPbrPipeline
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try {
                var version = GetVersion();
                Console.WriteLine($"Minecraft PBR-Pipeline {version}");

                await Host.CreateDefaultBuilder(args)
                    .ConfigureServices(ConfigureServices)
                    .UseSerilog(ConfigureLogging)
                    .RunConsoleAsync();

                return 0;
            }
            catch (Exception error) {
                Log.Logger.Fatal(error, "An unhandled exception has occurred!");
                return 1;
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IPublisher, Publisher>();
            services.AddSingleton<ITextureLoader, TextureLoader>();

            services.AddHostedService<TestService>();
        }

        private static void ConfigureLogging(HostBuilderContext context, LoggerConfiguration logger)
        {
            logger.Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.Console();
        }

        private static string GetVersion()
        {
            return Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }
    }
}
