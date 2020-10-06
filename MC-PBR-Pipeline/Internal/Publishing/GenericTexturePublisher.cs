using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class GenericTexturePublisher : TexturePublisherBase
    {
        public GenericTexturePublisher(PackProperties pack, IInputReader reader, IOutputWriter writer) : base(pack, reader, writer) {}

        public async Task PublishAsync(string filename, CancellationToken token)
        {
            var path = Path.GetDirectoryName(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            var newName = $"{name}.png";

            var destinationFile = path == null ? newName : Path.Combine(path, newName);

            await PublishAsync(filename, null, destinationFile, context => {
                Resize(context, null);
            }, token);
        }
    }
}
