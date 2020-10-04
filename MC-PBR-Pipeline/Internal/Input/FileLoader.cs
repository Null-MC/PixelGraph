using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
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
    internal class FileLoader
    {
        private readonly IInputReader reader;
        private readonly ILogger logger;


        public FileLoader(
            IServiceProvider provider,
            IInputReader reader)
        {
            this.reader = reader;

            logger = provider.GetRequiredService<ILogger<FileLoader>>();
        }

        public IAsyncEnumerable<object> LoadAsync(CancellationToken token = default)
        {
            return LoadRecursiveAsync(".", token);
        }

        private async IAsyncEnumerable<object> LoadRecursiveAsync(string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in reader.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                var mapFile = Path.Combine(directory, "pbr.properties");

                if (reader.FileExists(mapFile)) {
                    PbrProperties texture = null;

                    try {
                        texture = await LoadLocalTextureAsync(mapFile, token);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                    continue;
                }

                await foreach (var texture in LoadRecursiveAsync(directory, token))
                    yield return texture;

                var ignoreList = new List<string>();

                foreach (var filename in reader.EnumerateFiles(directory, "*.pbr")) {
                    PbrProperties texture = null;

                    try {
                        texture = await LoadGlobalTextureAsync(filename, token);

                        ignoreList.AddRange(texture.GetAllTextures(reader));
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    if (texture != null) yield return texture;
                }

                foreach (var filename in reader.EnumerateFiles(directory, "*")) {
                    if (ignoreList.Contains(filename, StringComparer.InvariantCultureIgnoreCase)) continue;

                    var extension = Path.GetExtension(filename);
                    if (IgnoredExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase)) {
                        logger.LogDebug($"Ignoring file '{filename}'.");
                        continue;
                    }

                    yield return filename;
                }
            }
        }

        public async Task<PbrProperties> LoadGlobalTextureAsync(string localFile, CancellationToken token = default)
        {
            var properties = new PbrProperties {
                Name = Path.GetFileNameWithoutExtension(localFile),
                Path = Path.GetDirectoryName(localFile),
                UseGlobalMatching = true,
            };

            await using var stream = reader.Open(localFile);
            await properties.ReadAsync(stream, token);

            return properties;
        }

        public async Task<PbrProperties> LoadLocalTextureAsync(string localFile, CancellationToken token = default)
        {
            var itemPath = Path.GetDirectoryName(localFile);

            var properties = new PbrProperties {
                Name = Path.GetFileName(itemPath),
                Path = Path.GetDirectoryName(itemPath),
            };

            await using var stream = reader.Open(localFile);
            await properties.ReadAsync(stream, token);

            return properties;
        }

        private static readonly string[] IgnoredExtensions = {".zip", ".db", ".cmd", ".sh", ".xcf", ".psd"};
    }
}
