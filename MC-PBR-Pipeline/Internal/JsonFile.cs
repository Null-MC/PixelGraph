using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal
{
    internal static class JsonFile
    {
        public static async Task<T> ReadAsync<T>(string filename, CancellationToken token)
        {
            await using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var data = await JToken.ReadFromAsync(jsonReader, token);

            return data.ToObject<T>();
        }

        public static async Task WriteAsync(Stream stream, object content, Formatting formatting, CancellationToken token)
        {
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer) {Formatting = formatting};

            await JToken.FromObject(content).WriteToAsync(jsonWriter, token);
        }

        public static async Task WriteAsync(string filename, object content, Formatting formatting, CancellationToken token)
        {
            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            await WriteAsync(stream, content, formatting, token);
        }

        //public static Task WriteAsync(string filename, object content, CancellationToken token)
        //{
        //    return WriteAsync(filename, content, Formatting.None, token);
        //}
    }
}
