using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class SpecularTexturePublisher : TexturePublisherBase
    {
        public SpecularTexturePublisher(IPublishProfile profile) : base(profile) {}

        public override async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path, texture.Name);
            var destinationFilename = Profile.GetDestinationPath(texture.Path, $"{texture.Name}_s.png");
            var specularMap = texture.Map.Specular;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Specular, sourcePath, texture.Map.Specular?.Texture),
            };

            if (texture.Map.Specular?.Color != null)
                filters.SourceColor = Rgba32.ParseHex(texture.Map.Specular.Color);

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

            if (texture.Map.Specular?.Metadata != null)
                await PublishMcMetaAsync(texture.Map.Specular.Metadata, destinationFilename, token);
        }
    }
}
