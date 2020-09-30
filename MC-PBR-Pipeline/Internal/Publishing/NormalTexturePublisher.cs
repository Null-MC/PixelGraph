using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
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
        public NormalTexturePublisher(IPublishProfile profile) : base(profile) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Profile.GetDestinationPath(texture.Path, $"{texture.Name}_n.png");
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

            if (existingNormal != null && File.Exists(existingNormal)) {
                filters.SourceFilename = existingNormal;
            }
            else if (fromHeight && existingHeight != null) {
                var options = new NormalMapOptions();

                if (normalMap?.DepthScale.HasValue ?? false)
                    options.DepthScale = normalMap.DepthScale.Value;

                if (normalMap?.Blur != null)
                    options.Blur = normalMap.Blur.Value;

                if (normalMap?.DownSample != null)
                    options.DownSample = normalMap.DownSample.Value;

                if (normalMap?.Strength != null)
                    options.Strength = normalMap.Strength.Value;

                if (normalMap?.Wrap != null)
                    options.Wrap = normalMap.Wrap.Value;

                filters.SourceFilename = existingHeight;
                filters.Append(new NormalMapFilter(options));
            }

            if (Profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = Profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (normalMap?.Metadata != null)
                await PublishMcMetaAsync(normalMap.Metadata, destinationFilename, token);
        }
    }
}
