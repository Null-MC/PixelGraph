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

        public async Task PublishAsync(IPbrProperties texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var sourceFile = GetFilename(texture, TextureTags.Albedo, sourcePath, texture.AlbedoTexture);
            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}.png");

            await PublishAsync(sourceFile, null, destinationFile, context => {
                Resize(context, texture);
            }, token);

            //if (albedoTexture?.Metadata != null)
            //    await PublishMcMetaAsync(albedoTexture.Metadata, destinationFile, token);
        }
    }
}
