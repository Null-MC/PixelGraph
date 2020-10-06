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
        public EmissiveTexturePublisher(PackProperties pack, IInputReader reader, IOutputWriter writer) : base(pack, reader, writer) {}

        public async Task PublishAsync(PbrProperties texture, CancellationToken token)
        {
            var sourceFile = texture.GetTextureFile(Reader, TextureTags.Emissive);
            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}_e.png");

            await PublishAsync(sourceFile, null, destinationFile, context => {
                // TODO: scale

                Resize(context, texture);
            }, token);

            //if (emissiveTexture?.Metadata != null)
            //    await PublishMcMetaAsync(emissiveTexture.Metadata, destinationFilename, token);
        }
    }
}
