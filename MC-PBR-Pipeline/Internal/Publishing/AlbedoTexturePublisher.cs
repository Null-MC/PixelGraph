using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class AlbedoTexturePublisher : TexturePublisherBase
    {
        public AlbedoTexturePublisher(IProfile profile, IOutputWriter output) : base(profile, output) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Path.Combine(texture.Path, $"{texture.Name}.png");
            var albedoTexture = texture.Map.Albedo;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Albedo, sourcePath, albedoTexture?.Texture),
            };

            if (Profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    Sampler = Profile.ResizeSampler,
                    TargetSize = Profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(Output, token);

            if (albedoTexture?.Metadata != null)
                await PublishMcMetaAsync(albedoTexture.Metadata, destinationFilename, token);
        }
    }
}
