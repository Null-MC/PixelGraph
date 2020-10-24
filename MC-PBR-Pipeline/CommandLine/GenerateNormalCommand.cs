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
    internal class GenerateNormalCommand
    {
        //private readonly IServiceProvider provider;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateNormalCommand(IServiceProvider provider)
        {
            //this.provider = provider;

            lifetime = provider.GetRequiredService<IAppLifetime>();
            logger = provider.GetRequiredService<ILogger<GenerateNormalCommand>>();

            Command = new Command("normal", "Generates a normal texture from a specified height texture.") {
                Handler = CommandHandler.Create<FileInfo, FileInfo, string>(RunAsync),
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
        }

        private async Task<int> RunAsync(FileInfo pbr, FileInfo height, string normal)
        {
            if (string.IsNullOrEmpty(normal)) {
                logger.LogError("Filename for normal output is undefined!");
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

            logger.LogDebug("Generating normals from height texture {heightFile}.", heightFile);
            var timer = Stopwatch.StartNew();

            using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, heightFile, lifetime.Token);

            var options = new NormalMapProcessor.Options {
                Source = heightTexture,
                // TODO: get channel from texture
                HeightChannel = ColorChannel.Red,
                Strength = texture?.NormalStrength ?? 1f,
                Noise = texture?.NormalNoise ?? 0f,
                Wrap = texture?.Wrap ?? true,
            };

            var processor = new NormalMapProcessor(options);
            using var normalTexture = new Image<Rgba32>(Configuration.Default, heightTexture.Width, heightTexture.Height);
            normalTexture.Mutate(c => c.ApplyProcessor(processor));

            await normalTexture.SaveAsync(normal, lifetime.Token);

            timer.Stop();
            var duration = timer.Elapsed.ToString("g");
            logger.LogInformation("Normal texture {normal} generated successfully. Duration: {duration}", normal, duration);

            return 0;
        }
    }
}
