using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class AlbedoTexturePublisher : TexturePublisherBase
    {
        public AlbedoTexturePublisher(
            IProfile profile,
            IInputReader reader,
            IOutputWriter writer) : base(profile, reader, writer) {}

        public async Task PublishAsync(PbrProperties texture, CancellationToken token)
        {
            var sourceFile = texture.GetTextureFile(Reader, TextureTags.Albedo);
            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}.png");

            await PublishAsync(sourceFile, null, destinationFile, context => {
                Resize(context, texture);
            }, token);

            //if (albedoTexture?.Metadata != null)
            //    await PublishMcMetaAsync(albedoTexture.Metadata, destinationFile, token);
        }
    }
}
