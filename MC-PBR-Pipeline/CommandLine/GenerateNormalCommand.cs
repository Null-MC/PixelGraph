using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Input;
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
        private readonly IServiceProvider provider;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateNormalCommand(IServiceProvider provider)
        {
            this.provider = provider;

            lifetime = provider.GetRequiredService<IAppLifetime>();
            logger = provider.GetRequiredService<ILogger<GenerateNormalCommand>>();

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
            if (string.IsNullOrEmpty(normal)) {
                logger.LogError("Filename for normal output is undefined!");
                return 1;
            }

            var pack = LoadPack(property);
            var texture = await LoadTextureAsync(pbr);
            var reader = new FileInputReader(pbr.DirectoryName);

            if (height?.Exists ?? false) {
                texture.Name = height.Name;
                texture.Properties["height.texture"] = height.FullName;
            }

            //logger.LogDebug("Generating normals from height texture {DisplayName}.", texture.DisplayName);
            var timer = Stopwatch.StartNew();

            try {
                var graphBuilder = new TextureGraphBuilder(provider, reader, null, pack);

                using var graph = graphBuilder.CreateGraph(texture);
                using var image = await graph.GetGeneratedNormalAsync(lifetime.Token);
                await image.SaveAsync(normal, lifetime.Token);

                logger.LogInformation("Normal texture {normal} generated successfully.", normal);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to generate Normal texture {normal}!", normal);
            }

            timer.Stop();
            var duration = timer.Elapsed.ToString("g");
            logger.LogDebug("Duration: {duration}", duration);

            return 0;
        }

        private static PackProperties LoadPack(string[] properties)
        {
            var pack = new PackProperties {
                Properties = {
                    ["input.format"] = "default",
                    ["output.format"] = "default",
                }
            };

            // TODO: load actual pack properties

            if (properties != null)
                foreach (var p in properties) pack.TrySet(p);

            return pack;
        }

        private async Task<PbrProperties> LoadTextureAsync(FileInfo textureFile)
        {
            var texture = new PbrProperties {
                UseGlobalMatching = false,
            };

            if (textureFile.Exists) {
                texture.UseGlobalMatching = !string.Equals(textureFile.Name, "pbr.properties", StringComparison.InvariantCultureIgnoreCase);
                
                texture.Name = texture.UseGlobalMatching
                    ? textureFile.Name
                    : textureFile.Directory?.Name;
                
                texture.Path = texture.UseGlobalMatching
                    ? "." : "..";

                await using var stream = textureFile.Open(FileMode.Open, FileAccess.Read);
                await texture.ReadAsync(stream, lifetime.Token);
            }

            return texture;
        }
    }
}
