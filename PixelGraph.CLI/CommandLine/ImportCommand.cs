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
        private readonly IServiceProvider provider;
        private readonly IAppLifetime lifetime;
        private readonly ILogger<ImportCommand> logger;

        public Command Command {get;}


        public ImportCommand(
            ILogger<ImportCommand> logger,
            IServiceProvider provider,
            IAppLifetime lifetime)
        {
            this.provider = provider;
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
            //factory.AddContentReader(ContentTypes.File);
            //factory.AddContentWriter(ContentTypes.File);
            //factory.AddTextureReader(GameEditions.Java);

            //factory.Services.AddTransient<Executor>();
            //await using var provider = factory.Build();
            var fullFilename = Path.GetFullPath(texture);

            ConsoleEx.WriteLine("\nImporting...", ConsoleColor.White);
            ConsoleEx.Write("  Texture     : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(fullFilename, ConsoleColor.Cyan);
            ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(destination?.FullName, ConsoleColor.Cyan);
            ConsoleEx.Write("  Format-In   : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(inputFormat, ConsoleColor.Cyan);
            ConsoleEx.Write("  Format-Out  : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(outputFormat, ConsoleColor.Cyan);
            ConsoleEx.WriteLine();

            var timer = Stopwatch.StartNew();

            try {

                var context = new ResourcePackContext {
                    Input = {
                        Format = inputFormat,
                    },
                    Profile = {
                        Encoding = {
                            Format = outputFormat,
                        }
                    },
                };

                var executor = provider.GetRequiredService<Executor>();
                executor.AsArchive = ;

                await executor.ExecuteAsync(context, root, fullFilename, destination?.FullName, inputFormat, outputFormat, property, lifetime.Token);

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
            finally {
                timer.Stop();

                ConsoleEx.Write("\nImport Duration: ", ConsoleColor.Gray);
                ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
            }
        }

        private class Executor
        {
            private readonly IServiceBuilder builder;
            public bool AsArchive {get; set;}


            public Executor(IServiceBuilder builder)
            {
                this.builder = builder;
            }

            public async Task ExecuteAsync(ResourcePackContext context, string sourcePath, string textureFilename, string destinationPath, string inputFormat, string outputFormat, string[] properties, CancellationToken token)
            {
                if (textureFilename == null) throw new ApplicationException("Source texture is undefined!");
                if (destinationPath == null) throw new ApplicationException("Destination path is undefined!");

                var edition = GameEdition.Parse(context.Profile.Edition);
                builder.AddContentReader(AsArchive ? ContentTypes.Archive : ContentTypes.File);
                builder.AddTextureReader(edition);

                builder.AddContentWriter(ContentTypes.File);
                builder.AddTextureWriter(GameEditions.None);

                builder.Services.AddTransient<Executor>();

                builder.Services.Configure<InputOptions>(options => {
                    options.Root = sourcePath;
                });

                await using var scope = builder.Build();

                var writer = scope.GetRequiredService<IOutputWriter>();
                var packWriter = scope.GetRequiredService<IResourcePackWriter>();

                writer.SetRoot(destinationPath);

                var graphContext = scope.GetRequiredService<ITextureGraphContext>();
                var graphBuilder = scope.GetRequiredService<IImportGraphBuilder>();

                graphContext.Input = context.Input;
                graphContext.Profile = context.Profile;
                graphContext.Material = new MaterialProperties {
                    Name = Path.GetFileName(textureFilename),
                    UseGlobalMatching = true,
                    LocalPath = ".",
                };

                await graphBuilder.ImportAsync(token);

                const string localFile = "mat.yml";
                await packWriter.WriteAsync(localFile, context.Profile, token);
            }
        }
    }
}
