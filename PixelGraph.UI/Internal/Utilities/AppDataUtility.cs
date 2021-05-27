using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.IO;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Utilities
{
    public interface IAppDataUtility
    {
        Task<string[]> ReadLinesAsync(string localFile, CancellationToken token = default);
        Task WriteLinesAsync(string localFile, IEnumerable<string> lines, CancellationToken token = default);
        Task<T> ReadJsonAsync<T>(string localFile, CancellationToken token = default);
        Task WriteJsonAsync(string localFile, object data, CancellationToken token = default);
    }

    internal class AppDataUtility : IAppDataUtility
    {
        public Task<string[]> ReadLinesAsync(string localFile, CancellationToken token = default)
        {
            var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);
            if (!File.Exists(fullFile)) return Task.FromResult<string[]>(null);

            return File.ReadAllLinesAsync(fullFile, token);
        }

        public Task WriteLinesAsync(string localFile, IEnumerable<string> lines, CancellationToken token = default)
        {
            var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);
            var finalPath = Path.GetDirectoryName(fullFile);

            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            return File.WriteAllLinesAsync(fullFile, lines, token);
        }

        public async Task<T> ReadJsonAsync<T>(string localFile, CancellationToken token = default)
        {
            using var reader = GetReader(localFile);
            if (reader == null) return default;

            using var jsonReader = new JsonTextReader(reader);
            var jsonData = await JToken.ReadFromAsync(jsonReader, token);
            return jsonData.ToObject<T>();
        }

        public async Task WriteJsonAsync(string localFile, object data, CancellationToken token = default)
        {
            await using var writer = GetWriter(localFile);
            using var jsonWriter = new JsonTextWriter(writer);

            var jsonData = JToken.FromObject(data);
            await jsonData.WriteToAsync(jsonWriter, token);
        }

        private static StreamReader GetReader(string localFile)
        {
            Stream stream = null;

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
            Stream stream = null;

            try {
                var fullFile = Path.Join(AppDataHelper.AppDataPath, localFile);
                var finalPath = Path.GetDirectoryName(fullFile);

                if (!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);

                stream = File.OpenWrite(fullFile);
                return new StreamWriter(stream);
            }
            catch {
                stream?.Dispose();
                throw;
            }
        }
    }
}
