using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public interface IPackReader
    {
        Task<PackProperties> ReadAsync(string filename, string[] additionalProperties = null, CancellationToken token = default);
    }

    internal class PackReader : IPackReader
    {
        public async Task<PackProperties> ReadAsync(string filename, string[] additionalProperties = null, CancellationToken token = default)
        {
            var pack = new PackProperties {
                Source = Path.GetDirectoryName(filename),
                WriteTime = File.GetLastWriteTime(filename),
                Properties = {
                    ["input.format"] = "default",
                    ["output.format"] = "default",
                }
            };

            await using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                await pack.ReadAsync(stream, token);
            }

            if (additionalProperties != null)
                foreach (var property in additionalProperties)
                    pack.TrySet(property);

            return pack;
        }
    }
}
