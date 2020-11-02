using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Publishing;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace McPbrPipeline.CommandLine
{
    internal class GenerateOcclusionCommand
    {
        private readonly ProviderFactory factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateOcclusionCommand(
            ProviderFactory factory,
            IAppLifetime lifetime,
            ILogger<GenerateOcclusionCommand> logger)
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
            await using var commandProvider = factory.Build(false);
            var reader = commandProvider.GetRequiredService<IInputReader>();
            var naming = commandProvider.GetRequiredService<INamingStructure>();
            var graphBuilder = commandProvider.GetRequiredService<ITextureGraphBuilder>();

            reader.SetRoot(pbr.DirectoryName);

            var packReader = new PackReader();
            var pack = await packReader.ReadAsync(pbr.FullName, property, lifetime.Token);
            
            var pbrReader = new PbrReader(reader);
            var textureList = await pbrReader.LoadAsync(pbr.Name, lifetime.Token);

            if (height?.Exists ?? false) {
                foreach (var texture in textureList) {
                    texture.Name = height.Name;
                    texture.Properties["height.texture"] = height.FullName;
                }
            }

            var timer = Stopwatch.StartNew();

            foreach (var texture in textureList) {
                logger.LogDebug("Generating ambient occlusion for texture {DisplayName}.", texture.DisplayName);
                var finalName = occlusion ?? naming.GetOutputTextureName(pack, texture, TextureTags.Occlusion, texture.UseGlobalMatching);

                try {
                    using var graph = graphBuilder.CreateGraph(pack, texture);
                    using var image = await graph.GetGeneratedOcclusionAsync(lifetime.Token);

                    await image.SaveAsync(finalName, lifetime.Token);
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

            return 0;
        }
    }
}
