using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    //public interface IPropertyReader
    //{
    //    Task ReadAsync(Stream stream, PropertiesFile properties, CancellationToken token = default);
    //}

    public interface IPropertyWriter
    {
        Task WriteAsync(Stream stream, PropertiesFile properties, CancellationToken token = default);
    }

    internal class PropertySerializer : IPropertyWriter
    {
        public async Task ReadAsync(Stream stream, PropertiesFile properties, CancellationToken token = default)
        {
            using var reader = new StreamReader(stream, null, true, -1, true);

            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
                token.ThrowIfCancellationRequested();

                line = line.Trim();
                if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;

                properties.TrySet(line);
            }
        }

        public async Task WriteAsync(Stream stream, PropertiesFile properties, CancellationToken token = default)
        {
            await using var writer = new StreamWriter(stream);

            var keys = properties.Properties.Keys;
            foreach (var key in keys.OrderBy(x => x)) {
                var value = properties.Properties[key];

                await writer.WriteAsync(key);
                await writer.WriteAsync(" = ");
                await writer.WriteLineAsync(value);
            }
        }
    }
}
