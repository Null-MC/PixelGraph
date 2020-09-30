using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class NormalTexturePublisher : TexturePublisherBase
    {
        public NormalTexturePublisher(IPublishProfile profile) : base(profile) {}

        public override async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path, texture.Name);
            var destinationFilename = Profile.GetDestinationPath(texture.Path, $"{texture.Name}_n.png");

            var fromHeight = texture.Map.Normal?.FromHeight ?? true;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
            };

            if (fromHeight) {
                filters.SourceFilename = GetFilename(texture, TextureTags.Height, sourcePath,
                    texture.Map.Normal?.Heightmap ?? texture.Map.Height?.Texture);

                var options = new NormalMapOptions();

                if (texture.Map.Normal?.DepthScale.HasValue ?? false)
                    options.DepthScale = texture.Map.Normal.DepthScale.Value;

                if (texture.Map.Normal?.Blur != null)
                    options.Blur = texture.Map.Normal.Blur.Value;

                if (texture.Map.Normal?.DownSample != null)
                    options.DownSample = texture.Map.Normal.DownSample.Value;

                if (texture.Map.Normal?.Strength != null)
                    options.Strength = texture.Map.Normal.Strength.Value;

                if (texture.Map.Normal?.Wrap != null)
                    options.Wrap = texture.Map.Normal.Wrap.Value;

                filters.Append(new NormalMapFilter(options));
            }
            else {
                filters.SourceFilename = GetFilename(texture, TextureTags.Normal, sourcePath, texture.Map.Normal?.Texture);
            }

            if (Profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = Profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (texture.Map.Normal?.Metadata != null)
                await PublishMcMetaAsync(texture.Map.Normal.Metadata, destinationFilename, token);
        }
    }
}
