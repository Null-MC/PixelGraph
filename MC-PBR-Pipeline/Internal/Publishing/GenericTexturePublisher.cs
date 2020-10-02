using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class GenericTexturePublisher : TexturePublisherBase
    {
        public GenericTexturePublisher(IProfile profile, IInputReader reader, IOutputWriter writer) : base(profile, reader, writer) {}

        public async Task PublishAsync(string filename, CancellationToken token)
        {
            var path = Path.GetDirectoryName(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            var newName = $"{name}.png";

            var filters = new FilterChain(Reader, Writer) {
                SourceFilename = filename,
                DestinationFilename = path == null ? newName : Path.Combine(path, newName),
            };

            Resize(filters, null);

            await filters.ApplyAsync(token);
        }
    }
}
