using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Serialization;
//public interface IPropertyWriter<T>
//    where T : new()
//{
//    Task<T> ReadAsync(StreamReader reader, CancellationToken token = default);
//    Task WriteAsync(StreamWriter writer, T properties, CancellationToken token = default);
//}

internal class PropertyFileSerializer //: IPropertyWriter<T>
{
    public string Separator {get; set;} = "=";


    public async IAsyncEnumerable<KeyValuePair<string, string>> ReadAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken token = default)
    {
        string line;
        while ((line = await reader.ReadLineAsync()) != null) {
            token.ThrowIfCancellationRequested();

            line = line.Trim();
            if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;

            if (!TryParseLine(line, out var propertyName, out var value))
                throw new ApplicationException($"Failed to parse property line '{line}'!");

            //SetProperty(data, propertyName, value);
            yield return new KeyValuePair<string, string>(propertyName, value);
        }
    }

    public async Task WriteAsync(StreamWriter writer, IEnumerable<KeyValuePair<string, string>> properties, CancellationToken token = default)
    {
        foreach (var (propertName, propertyValue) in properties) {
            token.ThrowIfCancellationRequested();
            if (propertyValue == null) continue;

            await writer.WriteAsync(propertName);
            await writer.WriteAsync(Separator);
            await writer.WriteLineAsync(propertyValue);
        }
    }

    private bool TryParseLine(string line, out string propertyName, out string value)
    {
        var i = line.IndexOf(Separator, StringComparison.InvariantCultureIgnoreCase);

        if (i < 0) {
            propertyName = value = null;
            return false;
        }

        propertyName = line[..i].TrimEnd();
        value = line[(i+Separator.Length)..].TrimStart();
        return true;
    }
}