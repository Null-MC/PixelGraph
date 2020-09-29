using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace McPbrPipeline.Internal.Textures
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
            var itemPath = Path.GetDirectoryName(filename);

            return new TextureCollection {
                Name = Path.GetFileName(itemPath),
                Path = Path.GetDirectoryName(itemPath)?[rootPath.Length..].TrimStart('\\', '/'),
                Map = await JsonFile.ReadAsync<TextureMap>(filename, token),
            };
        }
    }
}
