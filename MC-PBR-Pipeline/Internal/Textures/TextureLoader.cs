using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal interface ITextureLoader
    {
        IAsyncEnumerable<TextureCollection> LoadAsync(string path, CancellationToken token = default);
        //Task<TextureCollection> LoadTextureAsync(string rootPath, string filename, CancellationToken token = default);
    }

    internal class TextureLoader : ITextureLoader
    {
        private readonly ILogger logger;


        public TextureLoader(ILogger<TextureLoader> logger)
        {
            this.logger = logger;
        }

        public IAsyncEnumerable<TextureCollection> LoadAsync(string path, CancellationToken token = default)
        {
            return LoadRecursiveAsync(path, path, token);
        }

        private async IAsyncEnumerable<TextureCollection> LoadRecursiveAsync(string root, string path, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in Directory.EnumerateDirectories(path, "*")) {
                token.ThrowIfCancellationRequested();

                var mapFile = Path.Combine(directory, "pbr.json");

                if (File.Exists(mapFile)) {
                    TextureCollection texture;

                    try {
                        texture = await LoadLocalTextureAsync(root, mapFile, token);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                        continue;
                    }

                    yield return texture;
                }

                await foreach (var texture in LoadRecursiveAsync(root, directory, token))
                    yield return texture;

                var ignoreList = new List<string> {
                    Path.Combine(root, "pack.json"),
                };

                foreach (var filename in Directory.EnumerateFiles(path, "*.pbr")) {
                    var texture = await LoadGlobalTextureAsync(root, filename, token);
                    //ignoreList.Add(filename);

                    ignoreList.AddRange(Directory.EnumerateFiles(path, $"{texture.Name}.*"));
                    ignoreList.AddRange(Directory.EnumerateFiles(path, $"{texture.Name}_h.*"));
                    ignoreList.AddRange(Directory.EnumerateFiles(path, $"{texture.Name}_n.*"));
                    ignoreList.AddRange(Directory.EnumerateFiles(path, $"{texture.Name}_s.*"));

                    yield return texture;
                }

                foreach (var filename in Directory.EnumerateFiles(path, "*")) {
                    if (ignoreList.Contains(filename, StringComparer.InvariantCultureIgnoreCase)) continue;

                    var localFile = filename[root.Length..].TrimStart('\\', '/');
                    logger.LogInformation($"Found other file '{localFile}'.");
                }
            }
        }

        public async Task<TextureCollection> LoadGlobalTextureAsync(string rootPath, string filename, CancellationToken token = default)
        {
            return new TextureCollection {
                Name = Path.GetFileNameWithoutExtension(filename),
                Path = Path.GetDirectoryName(filename)?[rootPath.Length..].TrimStart('\\', '/'),
                Map = await JsonFile.ReadAsync<TextureMap>(filename, token),
                UseGlobalMatching = true,
            };
        }

        public async Task<TextureCollection> LoadLocalTextureAsync(string rootPath, string filename, CancellationToken token = default)
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
