using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class SpecularTexturePublisher : TexturePublisherBase
    {
        public SpecularTexturePublisher(IPublishProfile profile) : base(profile) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Profile.GetDestinationPath(texture.Path, $"{texture.Name}_s.png");
            var specularMap = texture.Map.Specular;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Specular, sourcePath, specularMap?.Texture),
            };

            if (specularMap?.Color != null)
                filters.SourceColor = Rgba32.ParseHex(specularMap.Color);

            if (specularMap?.HasScaling() ?? false) {
                // TODO: set channel min-max using material channel mapping
                var options = new ScaleOptions {
                    Red = specularMap.MetalScale,
                    Green = specularMap.SmoothScale,
                    Blue = specularMap.EmissiveScale,
                };

                filters.Append(new ScaleFilter(options));
            }

            if (specularMap?.HasOffsets() ?? false) {
                // TODO: set channel min-max using material channel mapping
                var options = new RangeOptions {
                    RedMin = specularMap.MetalMin,
                    RedMax = specularMap.MetalMax,
                    GreenMin = specularMap.SmoothMin,
                    GreenMax = specularMap.SmoothMax,
                    BlueMin = specularMap.EmissiveMin,
                    BlueMax = specularMap.EmissiveMax,
                };

                filters.Append(new RangeFilter(options));
            }

            if (Profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = Profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (specularMap?.Metadata != null)
                await PublishMcMetaAsync(specularMap.Metadata, destinationFilename, token);
        }
    }
}
