using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine
{
    internal class GenerateOcclusionCommand
    {
        private readonly IServiceBuilder factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateOcclusionCommand(
            ILogger<GenerateOcclusionCommand> logger,
            IServiceBuilder factory,
            IAppLifetime lifetime)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("occlusion", "Generates an ambient-occlusion texture from a specified height texture.") {
                Handler = CommandHandler.Create<FileInfo, FileInfo, string, string[]>(RunAsync),
            };

            Command.AddOption(new Option<FileInfo>(
                new [] {"--pbr"},
                () => new FileInfo("pbr.properties"),
                "The optional name of a PBR properties file containing settings for occlusion texture generation. Defaults to 'pbr.properties'."));

            Command.AddOption(new Option<FileInfo>(
                new [] {"-h", "--height"},
                "The name of the height texture to use for generating normals. Defaults to 'height.*'."));

            Command.AddOption(new Option<string>(
                new [] {"-ao", "--occlusion"},
                "The name of the occlusion texture to generate. Defaults to 'occlusion.png'."));

            Command.AddOption(new Option<string[]>(
                new[] {"--property" },
                "Override a pack property."));
        }

        private async Task<int> RunAsync(FileInfo pbr, FileInfo height, string occlusion, string[] property)
        {
            factory.AddFileInput();
            factory.AddFileOutput();

            factory.Services.AddTransient<Executor>();
            await using var provider = factory.Build();

            try {
                var executor = provider.GetRequiredService<Executor>();
                await executor.ExecuteAsync(pbr.FullName, height.FullName, occlusion, property, lifetime.Token);
                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
                return -1;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while generating occlusion texture!");
                return -1;
            }
        }

        private class Executor
        {
            private readonly ITextureGraphBuilder graphBuilder;
            private readonly INamingStructure naming;
            private readonly IInputReader reader;
            private readonly IPackReader packReader;
            private readonly IPbrReader pbrReader;
            private readonly ILogger logger;


            public Executor(
                ILogger<Executor> logger,
                ITextureGraphBuilder graphBuilder,
                INamingStructure naming,
                IInputReader reader,
                IPackReader packReader,
                IPbrReader pbrReader)
            {
                this.graphBuilder = graphBuilder;
                this.naming = naming;
                this.reader = reader;
                this.packReader = packReader;
                this.pbrReader = pbrReader;
                this.logger = logger;
            }

            public async Task ExecuteAsync(string pbrFilename, string heightFilename, string occlusionFilename, string[] properties, CancellationToken token = default)
            {
                var root = Path.GetDirectoryName(pbrFilename);
                var name = Path.GetFileName(pbrFilename);

                reader.SetRoot(root);

                var pack = await packReader.ReadAsync(pbrFilename, properties, token);
                var textureList = await pbrReader.LoadAsync(name, token);

                if (heightFilename != null && File.Exists(heightFilename)) {
                    var heightName = Path.GetFileName(heightFilename);

                    foreach (var texture in textureList) {
                        texture.Name = heightName;
                        texture.Properties["height.texture"] = heightFilename;
                    }
                }

                var timer = Stopwatch.StartNew();

                foreach (var texture in textureList) {
                    logger.LogDebug("Generating ambient occlusion for texture {DisplayName}.", texture.DisplayName);
                    var finalName = occlusionFilename ?? naming.GetOutputTextureName(pack, texture, TextureTags.Occlusion, texture.UseGlobalMatching);

                    try {
                        using var graph = graphBuilder.CreateGraph(pack, texture);
                        using var image = await graph.GetGeneratedOcclusionAsync(token);

                        await image.SaveAsync(finalName, token);
                        logger.LogInformation("Ambient Occlusion texture {finalName} generated successfully.", finalName);
                    }
                    catch (SourceEmptyException error) {
                        logger.LogError($"Failed to generate Ambient Occlusion texture {{finalName}}! {error.Message}", finalName);
                    }
                    catch (Exception error) {
                        logger.LogError(error, "Failed to generate Ambient Occlusion texture {finalName}!", finalName);
                    }
                }

                timer.Stop();
                var duration = timer.Elapsed.ToString("g");
                logger.LogDebug("Duration: {duration}", duration);
            }
        }
    }
}
