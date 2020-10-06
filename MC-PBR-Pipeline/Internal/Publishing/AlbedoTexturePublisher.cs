using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using McPbrPipeline.Internal.Encoding;

namespace McPbrPipeline.Internal.Publishing
{
    internal class AlbedoTexturePublisher : TexturePublisherBase
    {
        public AlbedoTexturePublisher(
            PackProperties pack,
            IInputReader reader,
            IOutputWriter writer) : base(pack, reader, writer) {}

        public async Task PublishAsync(PbrProperties texture, CancellationToken token)
        {
            var sourceFile = texture.GetTextureFile(Reader, TextureTags.Albedo);
            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}.png");

            var encoding = GetEncoding(texture);



            await PublishAsync(sourceFile, null, destinationFile, context => {
                Resize(context, texture);
            }, token);

            //if (albedoTexture?.Metadata != null)
            //    await PublishMcMetaAsync(albedoTexture.Metadata, destinationFile, token);
        }

        public TextureEncoding GetEncoding(PbrProperties texture)
        {
            return new TextureEncoding {
                Tag = TextureTags.Albedo,
                R = texture.AlbedoInputR,
                G = texture.AlbedoInputG,
                B = texture.AlbedoInputB,
                A = texture.AlbedoInputA,
            };
        }
    }
}
