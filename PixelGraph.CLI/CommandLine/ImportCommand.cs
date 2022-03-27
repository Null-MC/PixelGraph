using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine
{
    internal class ImportCommand
    {
        private readonly IServiceBuilder factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public ImportCommand(
            ILogger<ImportCommand> logger,
            IServiceBuilder factory,
            IAppLifetime lifetime)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("import", "Imports a texture from the specified source format to a destination format.") {
                Handler = CommandHandler.Create<string, DirectoryInfo, string, string, string[]>(RunAsync),
            };

            Command.AddOption(new Option<string>(
                new [] {"-t", "--texture"},
                "The name of the texture to import, excluding extension."));

            Command.AddOption(new Option<DirectoryInfo>(
                new[] { "-d", "--destination" },
                "The target directory to write the imported texture to."));

            Command.AddOption(new Option<string>(
                new[] { "-i", "--input-format" },
                "The format of the source texture."));

            Command.AddOption(new Option<string>(
                new[] { "-o", "--output-format" },
                "The target format of the imported texture."));

            Command.AddOption(new Option<string[]>(
                new[] { "--property" },
                "Override a pack property."));
        }

        private async Task<int> RunAsync(string texture, DirectoryInfo destination, string inputFormat, string outputFormat, string[] property)
        {
            factory.AddContentReader(ContentTypes.File);
            factory.AddContentWriter(ContentTypes.File);
            factory.AddTextureReader(GameEditions.Java);

            factory.Services.AddTransient<Executor>();
            await using var provider = factory.Build();

            try {
                var fullFilename = Path.GetFullPath(texture);

                var executor = provider.GetRequiredService<Executor>();
                await executor.ExecuteAsync(fullFilename, destination?.FullName, inputFormat, outputFormat, property, lifetime.Token);

                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
                return -1;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while converting!");
                return -1;
            }
        }

        private class Executor
        {
            private readonly IServiceProvider provider;
            private readonly IInputReader reader;
            private readonly IOutputWriter writer;
            private readonly IResourcePackWriter packWriter;


            public Executor(
                IServiceProvider provider,
                IInputReader reader,
                IOutputWriter writer,
                IResourcePackWriter packWriter)
            {
                this.provider = provider;
                this.reader = reader;
                this.writer = writer;
                this.packWriter = packWriter;
            }

            public async Task ExecuteAsync(string textureFilename, string destinationPath, string inputFormat, string outputFormat, string[] properties, CancellationToken token)
            {
                if (textureFilename == null) throw new ApplicationException("Source texture is undefined!");
                if (destinationPath == null) throw new ApplicationException("Destination path is undefined!");

                ConsoleEx.WriteLine("\nImporting...", ConsoleColor.White);
                ConsoleEx.Write("  Texture     : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(textureFilename, ConsoleColor.Cyan);
                ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(destinationPath, ConsoleColor.Cyan);
                ConsoleEx.Write("  Format-In   : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(inputFormat, ConsoleColor.Cyan);
                ConsoleEx.Write("  Format-Out  : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(outputFormat, ConsoleColor.Cyan);
                ConsoleEx.WriteLine();

                var timer = Stopwatch.StartNew();

                try {
                    var root = Path.GetDirectoryName(textureFilename);
                    reader.SetRoot(root);
                    writer.SetRoot(destinationPath);

                    var packInput = new ResourcePackInputProperties {
                        Format = inputFormat,
                    };

                    var packProfile = new ResourcePackProfileProperties {
                        Encoding = {
                            Format = outputFormat,
                        }
                    };

                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var graphBuilder = scope.ServiceProvider.GetRequiredService<IImportGraphBuilder>();

                    context.Input = packInput;
                    context.Profile = packProfile;
                    context.Material = new MaterialProperties {
                        Name = Path.GetFileName(textureFilename),
                        UseGlobalMatching = true,
                        LocalPath = ".",
                    };

                    await graphBuilder.ImportAsync(token);

                    const string localFile = "mat.yml";
                    await packWriter.WriteAsync(localFile, packProfile);
                }
                finally {
                    timer.Stop();

                    ConsoleEx.Write("\nImport Duration: ", ConsoleColor.Gray);
                    ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
                }
            }
        }
    }
}
