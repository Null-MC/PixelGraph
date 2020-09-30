using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class AlbedoTexturePublisher : TexturePublisherBase
    {
        public AlbedoTexturePublisher(IPublishProfile profile) : base(profile) {}

        public override async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path, texture.Name);
            var destinationFilename = Profile.GetDestinationPath(texture.Path, $"{texture.Name}.png");

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Albedo, sourcePath, texture.Map.Albedo?.Texture),
            };

            if (Profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = Profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (texture.Map.Albedo?.Metadata != null)
                await PublishMcMetaAsync(texture.Map.Albedo.Metadata, destinationFilename, token);
        }
    }
}
