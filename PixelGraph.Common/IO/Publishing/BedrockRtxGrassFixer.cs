using Microsoft.Extensions.Logging;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.Projects;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PixelGraph.Common.IO.Publishing;

internal class BedrockRtxGrassFixer
{
    private readonly ILogger<BedrockRtxGrassFixer> logger;
    private readonly IOutputWriter writer;
    private readonly IImageWriter imgWriter;


    public BedrockRtxGrassFixer(
        ILogger<BedrockRtxGrassFixer> logger,
        IOutputWriter writer,
        IImageWriter imgWriter)
    {
        this.logger = logger;
        this.writer = writer;
        this.imgWriter = imgWriter;
    }

    public async Task FixAsync(ProjectPublishContext packContext, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(packContext.Profile);

        var ext = NamingStructure.GetExtension(packContext.Profile);

        var grass_side_file = $"textures/blocks/grass_side.{ext}";
        if (!writer.FileExists(grass_side_file)) {
            logger.LogWarning("Unable to build grass_side texture, missing '{grass_side_file}'!", grass_side_file);
            return;
        }

        var grass_side_carried_file = $"textures/blocks/grass_side_carried.{ext}";
        if (!writer.FileExists(grass_side_carried_file)) {
            logger.LogWarning("Unable to build grass_side texture, missing '{grass_side_carried_file}'!", grass_side_carried_file);
            return;
        }

        var format = ImageWriter.GetFormat(ext);
        var encoder = imgWriter.GetEncoder(format, ImageChannels.ColorAlpha);

        //var colorFile = $"textures/blocks/grass_side.{ext}";
        using var overlayColorTex = await writer.OpenReadAsync(grass_side_carried_file,
            async stream => await Image.LoadAsync<Rgba32>(stream, token), token);

        await writer.OpenReadWriteAsync(grass_side_file, async stream => {
            using var colorTex = await Image.LoadAsync<Rgb24>(stream, token);

            colorTex.Mutate(imgContext => {
                var options = new BedrockGrassOpacityProcessor<Rgba32>.Options {
                    SamplerOverlay = new NearestSampler<Rgba32> {
                        Image = overlayColorTex,
                    },
                };

                options.SamplerOverlay.SetBounds(UVRegion.Full);

                // overlay RGB from overlayColorTex where colorTex opacity = 0
                var processor = new BedrockGrassOpacityProcessor<Rgba32>(options);
                imgContext.ApplyProcessor(processor);
            });

            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            await colorTex.SaveAsync(stream, encoder, token);
        }, token);

        // apply other layers using existing opacity mask
        var normalInputFile = $"textures/blocks/grass_side_carried_normal.{ext}";
        if (writer.FileExists(normalInputFile)) {
            using var overlayNormalTex = await writer.OpenReadAsync(normalInputFile,
                stream => Image.LoadAsync<Rgb24>(stream, token), token);

            await writer.OpenReadWriteAsync($"textures/blocks/grass_side_normal.{ext}", async stream => {
                using var normalTex = await Image.LoadAsync<Rgb24>(stream, token);

                normalTex.Mutate(imgContext => {
                    var options = new BedrockGrassOpacityProcessor<Rgb24>.Options {
                        SamplerOpacity = new NearestSampler<Rgba32> {
                            Image = overlayColorTex,
                        },
                        SamplerOverlay = new NearestSampler<Rgb24> {
                            Image = overlayNormalTex,
                        },
                    };

                    options.SamplerOpacity.SetBounds(UVRegion.Full);
                    options.SamplerOverlay.SetBounds(UVRegion.Full);

                    // overlay RGB from overlayColorTex where colorTex opacity = 0
                    var processor = new BedrockGrassOpacityProcessor<Rgb24>(options);
                    imgContext.ApplyProcessor(processor);
                });

                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                await normalTex.SaveAsync(stream, encoder, token);
            }, token);
        }

        var heightInputFile = $"textures/blocks/grass_side_carried_heightmap.{ext}";
        if (writer.FileExists(heightInputFile)) {
            using var overlayHeightTex = await writer.OpenReadAsync(heightInputFile,
                stream => Image.LoadAsync<L8>(stream, token), token);

            await writer.OpenReadWriteAsync($"textures/blocks/grass_side_heightmap.{ext}", async stream => {
                using var heightTex = await Image.LoadAsync<L8>(stream, token);

                heightTex.Mutate(imgContext => {
                    var options = new BedrockGrassOpacityProcessor<L8>.Options {
                        SamplerOpacity = new NearestSampler<Rgba32> {
                            Image = overlayColorTex,
                        },
                        SamplerOverlay = new NearestSampler<L8> {
                            Image = overlayHeightTex,
                        },
                    };

                    options.SamplerOpacity.SetBounds(UVRegion.Full);
                    options.SamplerOverlay.SetBounds(UVRegion.Full);

                    // overlay RGB from overlayColorTex where colorTex opacity = 0
                    var processor = new BedrockGrassOpacityProcessor<L8>(options);
                    imgContext.ApplyProcessor(processor);
                });

                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                await heightTex.SaveAsync(stream, encoder, token);
            }, token);
        }

        var merInputFile = $"textures/blocks/grass_side_carried_mer.{ext}";
        if (writer.FileExists(merInputFile)) {
            using var overlayMerTex = await writer.OpenReadAsync(merInputFile,
                stream => Image.LoadAsync<Rgb24>(stream, token), token);

            await writer.OpenReadWriteAsync($"textures/blocks/grass_side_mer.{ext}", async stream => {
                using var merTex = await Image.LoadAsync<Rgb24>(stream, token);

                merTex.Mutate(imgContext => {
                    var options = new BedrockGrassOpacityProcessor<Rgb24>.Options {
                        SamplerOpacity = new NearestSampler<Rgba32> {
                            Image = overlayColorTex,
                        },
                        SamplerOverlay = new NearestSampler<Rgb24> {
                            Image = overlayMerTex,
                        },
                    };

                    options.SamplerOpacity.SetBounds(UVRegion.Full);
                    options.SamplerOverlay.SetBounds(UVRegion.Full);

                    // overlay RGB from overlayColorTex where colorTex opacity = 0
                    var processor = new BedrockGrassOpacityProcessor<Rgb24>(options);
                    imgContext.ApplyProcessor(processor);
                });

                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                await merTex.SaveAsync(stream, encoder, token);
            }, token);
        }
    }
}