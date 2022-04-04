using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.CLI.CommandLine
{
    internal class GenerateOcclusionCommand
    {
        private readonly ILogger<GenerateOcclusionCommand> logger;
        private readonly IServiceBuilder factory;
        private readonly IAppLifetime lifetime;

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
                new [] {"--profile"},
                "The file name of the profile to publish."));

            Command.AddOption(new Option<FileInfo>(
                new [] {"-mat"},
                "The file name of the material to generate AO for."));

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

        private async Task<int> RunAsync(FileInfo profile, FileInfo height, string occlusion, string[] property)
        {
            //factory.AddContentReader(ContentTypes.File);
            //factory.AddContentWriter(ContentTypes.File);
            //factory.AddTextureReader(GameEditions.None);
            var root = Path.GetDirectoryName(profile.FullName) ?? ".";
            //var profileFile = Path.GetFileName(profile.FullName);
            //var inputName = PathEx.Join(root, "input.yml");

            //factory.Services.AddTransient<Executor>();
            //await using var provider = factory.Build();

            var timer = Stopwatch.StartNew();

            try {
                await using (var stream = profile.OpenRead()) {
                    context.Profile = ResourcePackReader.ParseProfile(stream);
                }

                var inputFile = PathEx.Join(root, "input.yml");
                await using (var stream = File.OpenRead(inputFile)) {
                    context.Input = ResourcePackReader.ParseInput(stream);
                }

                var executor = provider.GetRequiredService<Executor>();

                logger.LogDebug("Generating ambient occlusion for texture {DisplayName}.", material.DisplayName);

                await executor.ExecuteAsync(profile.FullName, height.FullName, occlusion, property, lifetime.Token);
                logger.LogInformation("Ambient Occlusion texture {outputName} generated successfully.", occlusionFilename);
                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
                return -1;
            }
            catch (SourceEmptyException) {
                logger.LogError("Unable to locate valid height source for ambient occlusion generation!");
                return -1;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while generating occlusion texture!");
                return -1;
            }
            finally {
                timer.Stop();
                var duration = timer.Elapsed.ToString("g");
                logger.LogDebug("Duration: {duration}", duration);
            }
        }

        private class Executor
        {
            private readonly IServiceBuilder builder;
            //private readonly IInputReader reader;
            //private readonly ITextureWriter texWriter;
            //private readonly IResourcePackReader packReader;
            //private readonly IMaterialReader materialReader;
            private readonly ILogger logger;


            public Executor(
                ILogger<Executor> logger,
                IServiceBuilder builder)
                //IInputReader reader,
                //ITextureWriter texWriter,
                //IResourcePackReader packReader,
                //IMaterialReader materialReader)
            {
                this.builder = builder;
                //this.reader = reader;
                //this.texWriter = texWriter;
                //this.packReader = packReader;
                //this.materialReader = materialReader;
                this.logger = logger;
            }

            public async Task ExecuteAsync(ResourcePackContext context, string sourcePath, string heightFilename, string occlusionFilename, string[] properties, CancellationToken token = default)
            {
                //var root = Path.GetDirectoryName(pbrFilename) ?? ".";
                //var packName = Path.GetFileName(pbrFilename);
                //var inputName = PathEx.Join(root, "input.yml");

                builder.Services.Configure<InputOptions>(options => {
                    options.Root = sourcePath;
                });

                await using var scope = builder.Build();

                //var packInput = await packReader.ReadInputAsync(inputName);
                //var packProfile = await packReader.ReadProfileAsync(pbrFilename);

                if (properties != null) {
                    // TODO: apply properties?
                }

                var matReader = scope.GetRequiredService<IMaterialReader>();
                var material = await matReader.LoadAsync(packName, token);

                if (heightFilename != null && File.Exists(heightFilename)) {
                    var heightName = Path.GetFileName(heightFilename);

                    material.Name = heightName;
                    material.Height.Texture = heightFilename;
                }

                if (occlusionFilename == null) {
                    var ext = NamingStructure.GetExtension(packProfile);
                    occlusionFilename = texWriter.TryGet(TextureTags.Occlusion, material.Name, ext, material.UseGlobalMatching);
                    if (occlusionFilename == null) {
                        // WARN: WHAT DO WE DO?!
                        throw new NotImplementedException();
                    }
                }

                logger.LogDebug("Generating ambient occlusion for texture {DisplayName}.", material.DisplayName);

                var graphContext = scope.GetRequiredService<ITextureGraphContext>();
                var occlusionGraph = scope.GetRequiredService<ITextureOcclusionGraph>();

                graphContext.Input = packInput;
                graphContext.Profile = packProfile;
                graphContext.Material = material;
                graphContext.InputEncoding = packInput.GetMapped().ToList();
                graphContext.OutputEncoding = packInput.GetMapped().ToList();

                using var occlusionImage = await occlusionGraph.GenerateAsync(token);

                await occlusionImage.SaveAsync(occlusionFilename, token);
                logger.LogInformation("Ambient Occlusion texture {outputName} generated successfully.", occlusionFilename);
            }
        }
    }
}
