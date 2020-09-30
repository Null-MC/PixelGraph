using McPbrPipeline.Internal.Publishing;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace McPbrPipeline
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
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

            services.AddSingleton<IAppCommandLine, AppCommandLine>();
            services.AddSingleton<ITextureLoader, TextureLoader>();
            services.AddSingleton<IPublisher, Publisher>();
        }
    }
}
