using System;
using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class NormalTexturePublisher : TexturePublisherBase
    {
        public NormalTexturePublisher(IProfile profile, IOutputWriter output) : base(profile, output) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Path.Combine(texture.Path, $"{texture.Name}_n.png");
            var normalMap = texture.Map.Normal;
            var heightMap = texture.Map.Height;

            var fromHeight = normalMap?.FromHeight ?? true;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
            };

            if (normalMap?.Angle != null && normalMap.Angle.Length == 3) {
                var vector = new Vector3(
                    normalMap.Angle[0],
                    normalMap.Angle[1],
                    normalMap.Angle[2]);

                MathEx.Normalize(ref vector);

                filters.SourceColor = new Rgba32(
                    vector.X * 0.5f + 0.5f,
                    vector.Y * 0.5f + 0.5f,
                    vector.Z);
            }

            var existingNormal = GetFilename(texture, TextureTags.Normal, sourcePath, normalMap?.Texture);
            var existingHeight = GetFilename(texture, TextureTags.Height, sourcePath, normalMap?.Heightmap ?? heightMap?.Texture);

            var depthScale = heightMap?.Scale ?? normalMap?.DepthScale ?? 1f;

            if (existingNormal != null && File.Exists(existingNormal)) {
                filters.SourceFilename = existingNormal;

                if (Math.Abs(depthScale - 1) > float.Epsilon) {
                    var options = new NormalMapOptions {
                        DepthScale = depthScale,
                    };

                    filters.Append(new NormalDepthFilter(options));
                }
            }
            else if (fromHeight && existingHeight != null) {
                var options = new NormalMapOptions();

                if (Math.Abs(depthScale - 1) > float.Epsilon)
                    options.DepthScale = depthScale;

                if (normalMap?.Blur != null)
                    options.Blur = normalMap.Blur.Value;

                if (normalMap?.DownSample != null)
                    options.DownSample = normalMap.DownSample.Value;

                if (normalMap?.Strength != null)
                    options.Strength = normalMap.Strength.Value;

                if (normalMap?.Wrap != null)
                    options.Wrap = normalMap.Wrap.Value;

                filters.SourceFilename = existingHeight;
                filters.Append(new NormalFilter(options));
            }

            if (Profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    Sampler = Profile.ResizeSampler,
                    TargetSize = Profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(Output, token);

            if (normalMap?.Metadata != null)
                await PublishMcMetaAsync(normalMap.Metadata, destinationFilename, token);
        }
    }
}
