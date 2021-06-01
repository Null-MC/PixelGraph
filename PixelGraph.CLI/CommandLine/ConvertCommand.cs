using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine
{
    internal class ConvertCommand
    {
        private readonly IServiceBuilder factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public ConvertCommand(
            ILogger<ConvertCommand> logger,
            IServiceBuilder factory,
            IAppLifetime lifetime)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("convert", "Converts a specified texture from a source format to a destination format.") {
                Handler = CommandHandler.Create<string, DirectoryInfo, string, string>(RunAsync),
            };

            Command.AddOption(new Option<string>(
                new [] {"-t", "--texture"},
                "The name of the texture to convert, excluding extension."));

            Command.AddOption(new Option<DirectoryInfo>(
                new[] { "-d", "--destination" },
                "The target directory to write the converted texture to."));

            Command.AddOption(new Option<string>(
                new[] { "-i", "--input-format" },
                "The format of the source texture."));

            Command.AddOption(new Option<string>(
                new[] { "-o", "--output-format" },
                "The target format of the converted texture."));
        }

        private async Task<int> RunAsync(string texture, DirectoryInfo destination, string inputFormat, string outputFormat)
        {
            factory.AddFileInput();
            factory.AddFileOutput();

            factory.Services.AddTransient<Executor>();
            await using var provider = factory.Build();
            
            try {
                var executor = provider.GetRequiredService<Executor>();
                await executor.ExecuteAsync(texture, destination?.FullName, inputFormat, outputFormat, lifetime.Token);
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
            //private readonly ITextureGraphBuilder graphBuilder;
            private readonly IResourcePackReader packReader;
            private readonly IInputReader reader;
            private readonly IOutputWriter writer;


            public Executor(
                IServiceProvider provider,
                //ITextureGraphBuilder graphBuilder,
                IResourcePackReader packReader,
                IInputReader reader,
                IOutputWriter writer)
            {
                this.provider = provider;
                //this.graphBuilder = graphBuilder;
                this.packReader = packReader;
                this.reader = reader;
                this.writer = writer;
            }

            public async Task ExecuteAsync(string textureFilename, string destinationPath, string inputFormat, string outputFormat, CancellationToken token = default)
            {
                if (textureFilename == null) throw new ApplicationException("Source texture is undefined!");
                if (destinationPath == null) throw new ApplicationException("Destination path is undefined!");

                ConsoleEx.WriteLine("\nConverting...", ConsoleColor.White);
                ConsoleEx.Write("  Texture     : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(textureFilename, ConsoleColor.Cyan);
                ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(destinationPath, ConsoleColor.Cyan);
                ConsoleEx.Write("  Format-In   : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(inputFormat, ConsoleColor.Cyan);
                ConsoleEx.Write("  Format-Out  : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(outputFormat, ConsoleColor.Cyan);
                ConsoleEx.WriteLine();

                var root = Path.GetDirectoryName(textureFilename);
                reader.SetRoot(root);
                writer.SetRoot(destinationPath);

                var fullFile = Path.GetFullPath(textureFilename);
                var packProfile = await packReader.ReadProfileAsync(fullFile);
                //pack.Properties["input.format"] = inputFormat;
                packProfile.Encoding.Format = outputFormat;

                var packInput = new ResourcePackInputProperties {
                    Format = TextureFormat.Format_Raw,
                };

                var material = new MaterialProperties {
                    UseGlobalMatching = true,
                    Name = Path.GetFileName(textureFilename),
                    LocalPath = ".",
                };

                var timer = Stopwatch.StartNew();

                try {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var graphBuilder = scope.ServiceProvider.GetRequiredService<ITextureGraphBuilder>();

                    context.Input = packInput;
                    context.Profile = packProfile;
                    context.Material = material;

                    await graphBuilder.ProcessInputGraphAsync(token);
                    //await graphBuilder.PublishInventoryAsync(token);
                }
                finally {
                    timer.Stop();

                    ConsoleEx.Write("\nConversion Duration: ", ConsoleColor.Gray);
                    ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
                }
            }
        }
    }
}
