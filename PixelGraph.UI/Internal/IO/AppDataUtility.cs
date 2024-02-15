using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.IO;
using System.IO;

namespace PixelGraph.UI.Internal.IO;

public interface IAppDataUtility
{
    Task<string[]> ReadLinesAsync(string localFile, CancellationToken token = default);
    Task WriteLinesAsync(string localFile, IEnumerable<string> lines, CancellationToken token = default);
    Task<T?> ReadJsonAsync<T>(string localFile, CancellationToken token = default);
    Task WriteJsonAsync(string localFile, object data, CancellationToken token = default);
    T? ReadJson<T>(string localFile);
}

internal class AppDataUtility : IAppDataUtility
{
    public Task<string[]> ReadLinesAsync(string localFile, CancellationToken token = default)
    {
        var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);

        return File.Exists(fullFile)
            ? File.ReadAllLinesAsync(fullFile, token)
            : Task.FromResult(Array.Empty<string>());
    }

    public Task WriteLinesAsync(string localFile, IEnumerable<string> lines, CancellationToken token = default)
    {
        var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);
        var finalPath = Path.GetDirectoryName(fullFile);

        if (finalPath != null && !Directory.Exists(finalPath))
            Directory.CreateDirectory(finalPath);

        return File.WriteAllLinesAsync(fullFile, lines, token);
    }

    public async Task<T?> ReadJsonAsync<T>(string localFile, CancellationToken token = default)
    {
        using var reader = GetReader(localFile);
        if (reader == null) return default;

        await using var jsonReader = new JsonTextReader(reader);
        var jsonData = await JToken.ReadFromAsync(jsonReader, token);
        return jsonData.ToObject<T>();
    }

    public async Task WriteJsonAsync(string localFile, object data, CancellationToken token = default)
    {
        await using var writer = GetWriter(localFile);
        await using var jsonWriter = new JsonTextWriter(writer);

        var jsonData = JToken.FromObject(data);
        await jsonData.WriteToAsync(jsonWriter, token);
    }

    public T? ReadJson<T>(string localFile)
    {
        using var reader = GetReader(localFile);
        if (reader == null) return default;

        using var jsonReader = new JsonTextReader(reader);
        var jsonData = JToken.ReadFrom(jsonReader);
        return jsonData.ToObject<T>();
    }

    private static StreamReader? GetReader(string localFile)
    {
        Stream? stream = null;

        try {
            var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);
            if (!File.Exists(fullFile)) return null;

            stream = File.OpenRead(fullFile);
            return new StreamReader(stream);
        }
        catch {
            stream?.Dispose();
            throw;
        }
    }

    private static StreamWriter GetWriter(string localFile)
    {
        Stream? stream = null;

        try {
            var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);
            var finalPath = Path.GetDirectoryName(fullFile);

            if (finalPath != null && !Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            stream = File.Open(fullFile, FileMode.Create, FileAccess.Write, FileShare.None);
            return new StreamWriter(stream);
        }
        catch {
            stream?.Dispose();
            throw;
        }
    }
}