using PixelGraph.Common.Encoding;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public interface IPackReader
    {
        Task<PackProperties> ReadAsync(string filename, string[] additionalProperties = null, CancellationToken token = default);
    }

    internal class PackReader : PropertySerializer, IPackReader
    {
        public async Task<PackProperties> ReadAsync(string filename, string[] additionalProperties = null, CancellationToken token = default)
        {
            var pack = new PackProperties {
                //Properties = {
                //    ["input.format"] = EncodingProperties.Default,
                //    ["output.format"] = EncodingProperties.Default,
                //},
            };

            await using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
            await ReadAsync(stream, pack, token);
            pack.Source = Path.GetDirectoryName(filename);
            pack.WriteTime = File.GetLastWriteTime(filename);
            
            if (additionalProperties != null)
                foreach (var property in additionalProperties)
                    pack.TrySet(property);

            return pack;
        }
    }
}
