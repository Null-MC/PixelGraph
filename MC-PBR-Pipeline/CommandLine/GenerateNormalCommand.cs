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
    internal class GenerateNormalCommand
    {
        private readonly ProviderFactory factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateNormalCommand(
            ProviderFactory factory,
            IAppLifetime lifetime,
            ILogger<GenerateNormalCommand> logger)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("normal", "Generates a normal texture from a specified height texture.") {
                Handler = CommandHandler.Create<FileInfo, FileInfo, string, string[]>(RunAsync),
            };

            Command.AddOption(new Option<FileInfo>(
                new [] {"--pbr"},
                () => new FileInfo("pbr.properties"),
                "The optional name of a PBR properties file containing settings for normal-texture generation. Defaults to 'pbr.properties'."));

            Command.AddOption(new Option<FileInfo>(
                new [] {"-h", "--height"},
                "The name of the height texture to use for generating normals. Defaults to 'height.*'."));

            Command.AddOption(new Option<string>(
                new [] {"-n", "--normal"},
                () => "normal.png",
                "The name of the normal texture to generate. Defaults to 'normal.png'."));

            Command.AddOption(new Option<string[]>(
                new[] {"--property" },
                "Override a pack property."));
        }

        private async Task<int> RunAsync(FileInfo pbr, FileInfo height, string normal, string[] property)
        {
            await using var commandProvider = factory.Build(false);
            var naming = commandProvider.GetRequiredService<INamingStructure>();
            var reader = commandProvider.GetRequiredService<IInputReader>();
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
                logger.LogDebug("Generating normals for texture {DisplayName}.", texture.DisplayName);
                var finalName = normal ?? naming.GetOutputTextureName(pack, texture, TextureTags.Normal, texture.UseGlobalMatching);

                try {
                    using var graph = graphBuilder.CreateGraph(pack, texture);
                    using var image = await graph.GetGeneratedNormalAsync(lifetime.Token);

                    await image.SaveAsync(finalName, lifetime.Token);
                    logger.LogInformation("Normal texture {finalName} generated successfully.", finalName);
                }
                catch (SourceEmptyException error) {
                    logger.LogError($"Failed to generate Normal texture {{finalName}}! {error.Message}", finalName);
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to generate Normal texture {finalName}!", finalName);
                }
            }

            timer.Stop();
            var duration = timer.Elapsed.ToString("g");
            logger.LogDebug("Duration: {duration}", duration);

            return 0;
        }
    }
}
