using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Textures
{
    internal interface ITextureLoader
    {
        IAsyncEnumerable<TextureCollection> LoadAsync(string path, CancellationToken token = default);
        Task<TextureCollection> LoadTextureAsync(string rootPath, string filename, CancellationToken token = default);
    }

    internal class TextureLoader : ITextureLoader
    {
        private readonly ILogger logger;


        public TextureLoader(ILogger<TextureLoader> logger)
        {
            this.logger = logger;
        }

        public async IAsyncEnumerable<TextureCollection> LoadAsync(string path, [EnumeratorCancellation] CancellationToken token = default)
        {
            TextureCollection texture;

            foreach (var filename in Directory.EnumerateFiles(path, "pbr.json", SearchOption.AllDirectories)) {
                token.ThrowIfCancellationRequested();

                try {
                    texture = await LoadTextureAsync(path, filename, token);
                }
                catch (Exception error) {
                    logger.LogWarning(error, $"Failed to load texture map '{filename}'!");
                    continue;
                }

                yield return texture;
            }
        }

        public async Task<TextureCollection> LoadTextureAsync(string rootPath, string filename, CancellationToken token = default)
        {
            var item_path = Path.GetDirectoryName(filename);
            var item_name = Path.GetFileName(item_path);
            var local_path = Path.GetDirectoryName(item_path)?[rootPath.Length..].TrimStart('\\', '/');

            return new TextureCollection {
                Name = item_name,
                Path = local_path,
                Map = await LoadMapAsync(filename, token),
            };
        }

        private static async Task<TextureMap> LoadMapAsync(string filename, CancellationToken token)
        {
            await using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var data = await JToken.ReadFromAsync(jsonReader, token);
            return data.ToObject<TextureMap>();
        }
    }
}
