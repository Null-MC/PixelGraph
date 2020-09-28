using McPbrPipeline.Internal.Extensions;
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
            var version = GetVersion();
            Console.WriteLine($"Minecraft PBR-Pipeline {version}");

            try {
                await Host.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .UseSerilog()
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

        private static string GetVersion()
        {
            return Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }
    }
}
