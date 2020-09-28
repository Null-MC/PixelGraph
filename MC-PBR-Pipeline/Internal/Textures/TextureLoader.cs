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
    internal class TextureLoader
    {
        private readonly ILogger logger;


        public TextureLoader(ILogger<TextureLoader> logger)
        {
            this.logger = logger;
        }

        public async IAsyncEnumerable<TextureCollection> LoadAsync(string path, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var filename in Directory.EnumerateFiles(path, "*.json")) {
                token.ThrowIfCancellationRequested();

                var texture = new TextureCollection {
                    Name = Path.GetFileNameWithoutExtension(filename),
                    Path = Path.GetDirectoryName(filename),
                };

                try {
                    texture.Map = await LoadMapAsync(filename, token);
                }
                catch (Exception error) {
                    logger.LogWarning(error, $"Failed to load texture map '{filename}'!");
                    continue;
                }

                yield return texture;
            }
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
