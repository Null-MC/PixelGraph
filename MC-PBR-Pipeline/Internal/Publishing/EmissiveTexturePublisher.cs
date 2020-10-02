using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class EmissiveTexturePublisher : TexturePublisherBase
    {
        public EmissiveTexturePublisher(IProfile profile, IInputReader reader, IOutputWriter writer) : base(profile, reader, writer) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Path.Combine(texture.Path, $"{texture.Name}_e.png");
            var emissiveTexture = texture.Map.Emissive;

            var filters = new FilterChain(Reader, Writer) {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Emissive, sourcePath, emissiveTexture?.Texture),
            };

            Resize(filters, texture);

            await filters.ApplyAsync(token);

            if (emissiveTexture?.Metadata != null)
                await PublishMcMetaAsync(emissiveTexture.Metadata, destinationFilename, token);
        }
    }
}
