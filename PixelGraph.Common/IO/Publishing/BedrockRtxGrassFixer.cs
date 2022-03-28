using Microsoft.Extensions.Logging;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
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

        public async Task FixAsync(ResourcePackContext packContext, CancellationToken token)
        {
            var ext = NamingStructure.GetExtension(packContext.Profile);

            if (!writer.FileExists($"textures/blocks/grass_side.{ext}")) {
                logger.LogWarning("Unable to build grass_side texture, missing grass_side!");
                return;
            }

            if (!writer.FileExists($"textures/blocks/grass_side_carried.{ext}")) {
                logger.LogWarning("Unable to build grass_side texture, missing grass_side_carried!");
                return;
            }

            var encoder = imgWriter.GetEncoder(ext, ImageChannels.ColorAlpha);

            await writer.OpenReadWriteAsync($"textures/blocks/grass_side.{ext}", async writeStream => {
                using var colorTex = await Image.LoadAsync<Rgba32>(Configuration.Default, writeStream, token);

                await writer.OpenReadAsync($"textures/blocks/grass_side_carried.{ext}", async readStream => {
                    using var overlayColorTex = await Image.LoadAsync<Rgb24>(Configuration.Default, readStream, token);

                    colorTex.Mutate(imgContext => {
                        var options = new BedrockGrassOpacityProcessor<Rgb24>.Options {
                            SamplerOverlay = new NearestSampler<Rgb24> {
                                Image = overlayColorTex,
                            },
                        };

                        // overlay RGB from overlayColorTex where colorTex opacity = 0
                        var processor = new BedrockGrassOpacityProcessor<Rgb24>(options);
                        imgContext.ApplyProcessor(processor);
                    });
                }, token);

                writeStream.Seek(0, SeekOrigin.Begin);
                writeStream.SetLength(0);
                await colorTex.SaveAsync(writeStream, encoder, token);

                // apply other layers using existing opacity mask
                if (writer.FileExists($"textures/blocks/grass_side_carried_normal.{ext}")) {
                    await writer.OpenReadWriteAsync($"textures/blocks/grass_side_normal.{ext}", async writeStream2 => {
                        using var normalTex = await Image.LoadAsync<Rgb24>(Configuration.Default, writeStream2, token);

                        await writer.OpenWriteAsync($"textures/blocks/grass_side_carried_normal.{ext}", async readStream => {
                            using var overlayNormalTex = await Image.LoadAsync<Rgb24>(Configuration.Default, readStream, token);

                            normalTex.Mutate(imgContext => {
                                var options = new BedrockGrassOpacityProcessor<Rgb24>.Options {
                                    SamplerOpacity = new NearestSampler<Rgba32> {
                                        Image = colorTex,
                                    },
                                    SamplerOverlay = new NearestSampler<Rgb24> {
                                        Image = overlayNormalTex,
                                    },
                                };

                                // overlay RGB from overlayColorTex where colorTex opacity = 0
                                var processor = new BedrockGrassOpacityProcessor<Rgb24>(options);
                                imgContext.ApplyProcessor(processor);
                            });
                        }, token);

                        writeStream2.Seek(0, SeekOrigin.Begin);
                        writeStream2.SetLength(0);
                        await normalTex.SaveAsync(writeStream2, encoder, token);
                    }, token);
                }

                if (writer.FileExists($"textures/blocks/grass_side_carried_heightmap.{ext}")) {
                    await writer.OpenReadWriteAsync($"textures/blocks/grass_side_heightmap.{ext}", async writeStream2 => {
                        using var heightTex = await Image.LoadAsync<L8>(Configuration.Default, writeStream2, token);

                        await writer.OpenWriteAsync($"textures/blocks/grass_side_carried_heightmap.{ext}", async readStream => {
                            using var overlayHeightTex = await Image.LoadAsync<L8>(Configuration.Default, readStream, token);

                            heightTex.Mutate(imgContext => {
                                var options = new BedrockGrassOpacityProcessor<L8>.Options {
                                    SamplerOpacity = new NearestSampler<Rgba32> {
                                        Image = colorTex,
                                    },
                                    SamplerOverlay = new NearestSampler<L8> {
                                        Image = overlayHeightTex,
                                    },
                                };

                                // overlay RGB from overlayColorTex where colorTex opacity = 0
                                var processor = new BedrockGrassOpacityProcessor<L8>(options);
                                imgContext.ApplyProcessor(processor);
                            });
                        }, token);

                        writeStream2.Seek(0, SeekOrigin.Begin);
                        writeStream2.SetLength(0);
                        await colorTex.SaveAsync(writeStream2, encoder, token);
                    }, token);
                }

                if (writer.FileExists($"textures/blocks/grass_side_carried_mer.{ext}")) {
                    await writer.OpenReadWriteAsync($"textures/blocks/grass_side_mer.{ext}", async writeStream2 => {
                        using var merTex = await Image.LoadAsync<Rgb24>(Configuration.Default, writeStream2, token);

                        await writer.OpenWriteAsync($"textures/blocks/grass_side_carried_mer.{ext}", async readStream => {
                            using var overlayMerTex = await Image.LoadAsync<Rgb24>(Configuration.Default, readStream, token);

                            merTex.Mutate(imgContext => {
                                var options = new BedrockGrassOpacityProcessor<Rgb24>.Options {
                                    SamplerOpacity = new NearestSampler<Rgba32> {
                                        Image = colorTex,
                                    },
                                    SamplerOverlay = new NearestSampler<Rgb24> {
                                        Image = overlayMerTex,
                                    },
                                };

                                // overlay RGB from overlayColorTex where colorTex opacity = 0
                                var processor = new BedrockGrassOpacityProcessor<Rgb24>(options);
                                imgContext.ApplyProcessor(processor);
                            });
                        }, token);

                        writeStream2.Seek(0, SeekOrigin.Begin);
                        writeStream2.SetLength(0);
                        await colorTex.SaveAsync(writeStream2, encoder, token);
                    }, token);
                }
            }, token);
        }
    }
}
