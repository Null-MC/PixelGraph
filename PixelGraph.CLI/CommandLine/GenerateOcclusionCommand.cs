using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            private readonly IServiceProvider provider;
            //private readonly INamingStructure naming;
            private readonly IInputReader reader;
            private readonly IResourcePackReader packReader;
            private readonly IMaterialReader materialReader;
            private readonly ILogger logger;


            public Executor(
                ILogger<Executor> logger,
                IServiceProvider provider,
                //INamingStructure naming,
                IInputReader reader,
                IResourcePackReader packReader,
                IMaterialReader materialReader)
            {
                this.provider = provider;
                //this.naming = naming;
                this.reader = reader;
                this.packReader = packReader;
                this.materialReader = materialReader;
                this.logger = logger;
            }

            public async Task ExecuteAsync(string pbrFilename, string heightFilename, string occlusionFilename, string[] properties, CancellationToken token = default)
            {
                var root = Path.GetDirectoryName(pbrFilename) ?? ".";
                var packName = Path.GetFileName(pbrFilename);
                var inputName = PathEx.Join(root, "input.yml");

                reader.SetRoot(root);

                var packInput = await packReader.ReadInputAsync(inputName);
                var packProfile = await packReader.ReadProfileAsync(pbrFilename);

                if (properties != null) {
                    // TODO: apply properties?
                }

                var material = await materialReader.LoadAsync(packName, token);

                if (heightFilename != null && File.Exists(heightFilename)) {
                    var heightName = Path.GetFileName(heightFilename);

                    material.Name = heightName;
                    material.Height.Texture = heightFilename;
                }

                var timer = Stopwatch.StartNew();

                if (occlusionFilename == null) {
                    //var outputName = occlusionFilename ?? NamingStructure.GetOutputTextureName(packProfile, material.Name, TextureTags.Occlusion, material.UseGlobalMatching);
                    var ext = NamingStructure.GetExtension(packProfile);
                    occlusionFilename = NamingStructure.Get(TextureTags.Occlusion, material.Name, ext, material.UseGlobalMatching);
                }

                logger.LogDebug("Generating ambient occlusion for texture {DisplayName}.", material.DisplayName);

                try {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var occlusionGraph = provider.GetRequiredService<ITextureOcclusionGraph>();

                    context.Input = packInput;
                    context.Profile = packProfile;
                    context.Material = material;
                    context.InputEncoding = packInput.GetMapped().ToList();
                    context.OutputEncoding = packInput.GetMapped().ToList();

                    using var occlusionImage = await occlusionGraph.GenerateAsync(token);

                    await occlusionImage.SaveAsync(occlusionFilename, token);
                    logger.LogInformation("Ambient Occlusion texture {outputName} generated successfully.", occlusionFilename);
                }
                catch (SourceEmptyException) {
                    logger.LogError("Unable to locate valid height source for ambient occlusion generation!");
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to generate Ambient Occlusion texture {outputName}!", occlusionFilename);
                }

                timer.Stop();
                var duration = timer.Elapsed.ToString("g");
                logger.LogDebug("Duration: {duration}", duration);
            }
        }
    }
}
