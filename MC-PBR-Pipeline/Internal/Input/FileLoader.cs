using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Input
{
    internal interface IFileLoader
    {
        IAsyncEnumerable<object> LoadAsync(string path, CancellationToken token = default);
    }

    internal class FileLoader : IFileLoader
    {
        private readonly ILogger logger;


        public FileLoader(ILogger<FileLoader> logger)
        {
            this.logger = logger;
        }

        public IAsyncEnumerable<object> LoadAsync(string rootPath, CancellationToken token = default)
        {
            return LoadRecursiveAsync(rootPath, rootPath, token);
        }

        private async IAsyncEnumerable<object> LoadRecursiveAsync(string rootPath, string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in Directory.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                var mapFile = Path.Combine(directory, "pbr.json");

                if (File.Exists(mapFile)) {
                    TextureCollection texture = null;

                    try {
                        texture = await LoadLocalTextureAsync(rootPath, mapFile, token);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                    continue;
                }

                await foreach (var texture in LoadRecursiveAsync(rootPath, directory, token))
                    yield return texture;

                var ignoreList = new List<string> {
                    Path.Combine(rootPath, "pack.json"),
                };

                foreach (var filename in Directory.EnumerateFiles(directory, "*.pbr")) {
                    TextureCollection texture = null;

                    try {
                        texture = await LoadGlobalTextureAsync(rootPath, filename, token);

                        ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}.*"));
                        ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}_h.*"));
                        ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}_n.*"));
                        ignoreList.AddRange(Directory.EnumerateFiles(directory, $"{texture.Name}_s.*"));
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                }

                foreach (var filename in Directory.EnumerateFiles(directory, "*")) {
                    if (ignoreList.Contains(filename, StringComparer.InvariantCultureIgnoreCase)) continue;

                    var extension = Path.GetExtension(filename);
                    if (IgnoredExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                        continue;

                    yield return filename[rootPath.Length..].TrimStart('\\', '/');
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

        private static readonly string[] IgnoredExtensions = {".zip", ".db", ".cmd", ".sh", ".xcf", ".psd"};
    }
}
