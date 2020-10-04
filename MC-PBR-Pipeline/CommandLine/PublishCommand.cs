using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace McPbrPipeline.CommandLine
{
    internal class PublishCommand
    {
        private readonly IServiceProvider provider;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public PublishCommand(IServiceProvider provider)
        {
            this.provider = provider;

            lifetime = provider.GetRequiredService<IAppLifetime>();
            logger = provider.GetRequiredService<ILogger<PublishCommand>>();

            Command = new Command("publish", "Publishes the specified profile.") {
                Handler = CommandHandler.Create<FileInfo, DirectoryInfo, bool, bool>(RunAsync),
            };

            Command.AddOption(new Option<FileInfo>(
                new [] {"-p", "--profile"},
                () => new FileInfo("pack.json"),
                "The file name of the profile to publish."));

            Command.AddOption(new Option<DirectoryInfo>(
                new[] { "-d", "--destination" },
                "The target directory to publish the resource pack to."));

            Command.AddOption(new Option<bool>(
                new [] {"-c", "--clean"},
                () => false,
                "Generates a compressed ZIP archive of the published contents."));

            Command.AddOption(new Option<bool>(
                new [] {"-z", "--zip"},
                () => false,
                "Generates a compressed ZIP archive of the published contents."));
        }

        private async Task<int> RunAsync(FileInfo profile, DirectoryInfo destination, bool clean, bool zip)
        {
            if (profile == null) {
                ConsoleEx.WriteLine("profile is undefined!", ConsoleColor.DarkRed);
                return -1;
            }

            if (destination == null) {
                ConsoleEx.WriteLine("Destination is undefined!", ConsoleColor.DarkRed);
                return -1;
            }

            ConsoleEx.WriteLine("\nPublishing...", ConsoleColor.White);
            ConsoleEx.Write("  Profile     : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(profile.FullName, ConsoleColor.Cyan);
            ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(destination.FullName, ConsoleColor.Cyan);
            ConsoleEx.WriteLine();

            var options = new PublishOptions {
                Profile = profile.FullName,
                Destination = destination.FullName,
                Compress = zip,
                Clean = clean,
            };

            var timer = Stopwatch.StartNew();

            try {
                var publisher = provider.GetRequiredService<IPublisher>();
                await publisher.PublishAsync(options, lifetime.Token);
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while publishing!");
            }
            finally {
                timer.Stop();
            }

            ConsoleEx.Write("\nPublish Duration: ", ConsoleColor.Gray);
            ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
            return 0;
        }
    }
}
