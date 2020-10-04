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

        public async Task PublishAsync(IPbrProperties texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var sourceFile = GetFilename(texture, TextureTags.Emissive, sourcePath, texture.EmissiveTexture);
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
