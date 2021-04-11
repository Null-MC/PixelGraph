using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine
{
    internal class PublishCommand
    {
        private readonly IServiceBuilder factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public PublishCommand(
            ILogger<PublishCommand> logger,
            IServiceBuilder factory,
            IAppLifetime lifetime)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("publish", "Publishes the specified profile.") {
                Handler = CommandHandler.Create<FileInfo, DirectoryInfo, FileInfo, bool>(RunAsync),
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
        }

        private async Task<int> RunAsync(FileInfo profile, DirectoryInfo destination, FileInfo zip, bool clean)
        {
            factory.AddFileInput();
            if (zip != null) factory.AddArchiveOutput();
            else factory.AddFileOutput();

            factory.Services.AddTransient<Executor>();
            await using var provider = factory.Build();

            try {
                var destPath = zip?.FullName ?? destination.FullName;

                var executor = provider.GetRequiredService<Executor>();
                executor.CleanDestination = clean;

                await executor.ExecuteAsync(profile.FullName, destPath, lifetime.Token);

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
        }

        private class Executor
        {
            private readonly IInputReader reader;
            private readonly IOutputWriter writer;
            private readonly IResourcePackReader packReader;
            private readonly IPublisher publisher;

            public bool CleanDestination {get; set;}


            public Executor(
                IResourcePackReader packReader,
                IInputReader reader,
                IOutputWriter writer,
                IPublisher publisher)
            {
                this.packReader = packReader;
                this.reader = reader;
                this.writer = writer;
                this.publisher = publisher;
            }

            public async Task ExecuteAsync(string packFilename, string destFilename, CancellationToken token = default)
            {
                if (packFilename == null) throw new ApplicationException("Pack profile is undefined!");
                if (destFilename == null) throw new ApplicationException("Either Destination or Zip must be defined!");

                ConsoleEx.WriteLine("\nPublishing...", ConsoleColor.White);
                ConsoleEx.Write("  Profile     : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(packFilename, ConsoleColor.Cyan);
                ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(destFilename, ConsoleColor.Cyan);
                ConsoleEx.WriteLine();

                var root = Path.GetDirectoryName(packFilename);
                var localFile = Path.GetFileName(packFilename);

                reader.SetRoot(root);
                writer.SetRoot(destFilename);

                var timer = Stopwatch.StartNew();

                try {
                    writer.Prepare();

                    var packProfile = await packReader.ReadProfileAsync(localFile);
                    var packInput = await packReader.ReadInputAsync("input.yml");

                    var context = new ResourcePackContext {
                        Input = packInput,
                        Profile = packProfile,
                    };

                    await publisher.PublishAsync(context, CleanDestination, token);
                }
                finally {
                    timer.Stop();

                    ConsoleEx.Write("\nPublish Duration: ", ConsoleColor.Gray);
                    ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
                }
            }
        }
    }
}
