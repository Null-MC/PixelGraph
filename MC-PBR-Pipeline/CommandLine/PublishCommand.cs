using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Publishing;
using McPbrPipeline.Internal.Textures;
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
        private readonly ProviderFactory factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public PublishCommand(
            ProviderFactory factory,
            IAppLifetime lifetime,
            ILogger<GenerateNormalCommand> logger)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("publish", "Publishes the specified profile.") {
                Handler = CommandHandler.Create<FileInfo, DirectoryInfo, FileInfo, bool, string[]>(RunAsync),
            };

            Command.AddOption(new Option<FileInfo>(
                new [] {"-p", "--profile"},
                () => new FileInfo("pack.json"),
                "The file name of the profile to publish."));

            Command.AddOption(new Option<DirectoryInfo>(
                new[] { "-d", "--destination" },
                "The target directory to publish the resource pack to."));

            Command.AddOption(new Option<string>(
                new [] {"-z", "--zip"},
                "Generates a compressed ZIP archive of the published contents."));

            Command.AddOption(new Option<bool>(
                new [] {"-c", "--clean"},
                () => false,
                "Generates a compressed ZIP archive of the published contents."));

            Command.AddOption(new Option<string[]>(
                new[] {"--property" },
                "Override a pack property."));
        }

        private async Task<int> RunAsync(FileInfo profile, DirectoryInfo destination, FileInfo zip, bool clean, string[] property)
        {
            if (profile == null) {
                ConsoleEx.WriteLine("profile is undefined!", ConsoleColor.DarkRed);
                return -1;
            }

            if (destination == null && zip == null) {
                ConsoleEx.WriteLine("Either Destination or Zip must be defined!", ConsoleColor.DarkRed);
                return -1;
            }

            var destPath = zip?.FullName ?? destination.FullName;
            ConsoleEx.WriteLine("\nPublishing...", ConsoleColor.White);
            ConsoleEx.Write("  Profile     : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(profile.FullName, ConsoleColor.Cyan);
            ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(destPath, ConsoleColor.Cyan);
            ConsoleEx.WriteLine();

            await using var commandProvider = factory.Build(zip != null);
            var reader = commandProvider.GetRequiredService<IInputReader>();
            var writer = commandProvider.GetRequiredService<IOutputWriter>();
            var graphBuilder = commandProvider.GetRequiredService<ITextureGraphBuilder>();

            var packReader = new PackReader();
            var pack = await packReader.ReadAsync(profile.FullName, property, lifetime.Token);

            graphBuilder.UseGlobalOutput = true;
            reader.SetRoot(pack.Source);
            writer.SetRoot(destPath);

            var timer = Stopwatch.StartNew();

            try {
                writer.Prepare();

                var publisher = commandProvider.GetRequiredService<IPublisher>();
                await publisher.PublishAsync(pack, destPath, clean);
                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
                return -1;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while publishing!");
                return -1;
            }
            finally {
                timer.Stop();

                ConsoleEx.Write("\nDuration: ", ConsoleColor.Gray);
                ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
            }
        }
    }
}
