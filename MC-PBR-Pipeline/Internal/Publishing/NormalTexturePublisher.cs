using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class NormalTexturePublisher : TexturePublisherBase
    {
        public NormalTexturePublisher(PackProperties pack, IInputReader reader, IOutputWriter writer) : base(pack, reader, writer) {}

        public async Task PublishAsync(PbrProperties texture, CancellationToken token)
        {
            Rgba32? sourceColor = null;
            if (TryGetNormalAngle(texture, out var normalAngle)) {
                MathEx.Normalize(ref normalAngle);

                sourceColor = new Rgba32(
                    normalAngle.X * 0.5f + 0.5f,
                    normalAngle.Y * 0.5f + 0.5f,
                    normalAngle.Z);
            }

            var existingNormal = texture.GetTextureFile(Reader, TextureTags.Normal);
            var existingHeight = texture.GetTextureFile(Reader, TextureTags.Height);

            string sourceFile = null;
            if (existingNormal != null && Reader.FileExists(existingNormal)) {
                sourceFile = existingNormal;
            }
            else if (texture.NormalFromHeight && existingHeight != null) {
                sourceFile = existingHeight;
            }

            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}_n.png");
            await PublishAsync(sourceFile, sourceColor, destinationFile, context => {
                if (existingNormal != null && Reader.FileExists(existingNormal)) {
                    if (Math.Abs(texture.NormalDepthScale - 1) > float.Epsilon) {
                        var options = new NormalMapOptions {
                            DepthScale = texture.NormalDepthScale,
                        };

                        var processor = new NormalDepthProcessor(options);
                        context.ApplyProcessor(processor);
                    }
                }
                else if (texture.NormalFromHeight && existingHeight != null) {
                    var normalOptions = new NormalMapOptions {
                        Wrap = texture.Wrap,
                        Strength = texture.NormalStrength,
                        DepthScale = texture.NormalDepthScale,
                    };

                    //if (texture.NormalBlur != null)
                    //    normalOptions.Blur = texture.NormalBlur;

                    //if (texture.NormalDownSample != null)
                    //    normalOptions.DownSample = texture.NormalDownSample.Value;

                    var chain = new List<IImageProcessor>();
                    var sourceSize = context.GetCurrentSize();
                    var downSampleSize = new Size();

                    if (normalOptions.DownSample > 1) {
                        downSampleSize.Width = sourceSize.Width / normalOptions.DownSample;
                        downSampleSize.Height = sourceSize.Height / normalOptions.DownSample;

                        var resizeOptions = new ResizeOptions {
                            Mode = ResizeMode.Stretch,
                            Sampler = KnownResamplers.Bicubic,
                            Size = downSampleSize,
                        };

                        chain.Add(new ResizeProcessor(resizeOptions, sourceSize));

                        // TODO: Re-normalize after resize!
                    }

                    if (normalOptions.Blur > float.Epsilon) {
                        chain.Add(new GaussianBlurProcessor(normalOptions.Blur));
                    }

                    chain.Add(new NormalMapProcessor(normalOptions));

                    context.ApplyProcessors(chain.ToArray());
                }

                Resize(context, texture);

                // TODO: Re-normalize after resize!
            }, token);

            //if (normalMap?.Metadata != null)
            //    await PublishMcMetaAsync(normalMap.Metadata, destinationFilename, token);
        }

        private static bool TryGetNormalAngle(PbrProperties texture, out Vector3 angle)
        {
            if (!texture.NormalX.HasValue && !texture.NormalY.HasValue && !texture.NormalZ.HasValue) {
                angle = Vector3.Zero;
                return false;
            }

            angle.X = texture.NormalX ?? 0f;
            angle.Y = texture.NormalY ?? 0f;
            angle.Z = texture.NormalZ ?? 0f;
            MathEx.Normalize(ref angle);
            return true;
        }
    }
}
