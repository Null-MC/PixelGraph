using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
        //private readonly IServiceProvider provider;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateOcclusionCommand(IServiceProvider provider)
        {
            //this.provider = provider;

            lifetime = provider.GetRequiredService<IAppLifetime>();
            logger = provider.GetRequiredService<ILogger<GenerateOcclusionCommand>>();

            Command = new Command("occlusion", "Generates an ambient-occlusion texture from a specified height texture.") {
                Handler = CommandHandler.Create<FileInfo, FileInfo, string>(RunAsync),
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
                () => "occlusion.png",
                "The name of the occlusion texture to generate. Defaults to 'occlusion.png'."));
        }

        private async Task<int> RunAsync(FileInfo pbr, FileInfo height, string occlusion)
        {
            if (string.IsNullOrEmpty(occlusion)) {
                logger.LogError("Filename for occlusion output is undefined!");
                return 1;
            }

            PbrProperties texture = null;

            if (pbr.Exists) {
                await using var stream = pbr.Open(FileMode.Open, FileAccess.Read);

                texture = new PbrProperties();
                await texture.ReadAsync(stream, lifetime.Token);
            }

            var heightFile = height?.FullName;
            if (heightFile == null && texture != null) {
                var reader = new FileInputReader(pbr.DirectoryName);
                heightFile = texture.GetTextureFile(reader, TextureTags.Height);
            }

            if (heightFile == null) {
                logger.LogError("Height texture file not found!");
                return 1;
            }

            logger.LogDebug("Generating ambient occlusion from height texture {heightFile}.", heightFile);
            var timer = Stopwatch.StartNew();

            using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, heightFile, lifetime.Token);

            var options = new OcclusionProcessor.Options {
                Source = heightTexture,
                // TODO: get channel from texture
                HeightChannel = ColorChannel.Red,
                StepCount = texture?.OcclusionSteps ?? 8,
                Quality = texture?.OcclusionQuality ?? 0.1f,
                ZScale = texture?.OcclusionZScale ?? 10f,
                Wrap = texture?.Wrap ?? true,
            };

            var processor = new OcclusionProcessor(options);
            using var occlusionTexture = new Image<Rgba32>(Configuration.Default, heightTexture.Width, heightTexture.Height);
            occlusionTexture.Mutate(c => c.ApplyProcessor(processor));

            await occlusionTexture.SaveAsync(occlusion, lifetime.Token);

            timer.Stop();
            var duration = timer.Elapsed.ToString("g");
            logger.LogInformation("Ambient Occlusion texture {occlusion} generated successfully. Duration: {duration}", occlusion, duration);

            return 0;
        }
    }
}
