using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PixelGraph.Common.IO
{
    public interface IFileLoader
    {
        IAsyncEnumerable<object> LoadAsync(CancellationToken token = default);
    }

    internal class FileLoader : IFileLoader
    {
        private readonly IInputReader reader;
        private readonly IPbrReader pbrReader;
        private readonly ILogger logger;


        public FileLoader(
            IInputReader reader,
            IPbrReader pbrReader,
            ILogger<FileLoader> logger)
        {
            this.reader = reader;
            this.pbrReader = pbrReader;
            this.logger = logger;
        }

        public IAsyncEnumerable<object> LoadAsync(CancellationToken token = default)
        {
            return LoadRecursiveAsync(".", token);
        }

        private async IAsyncEnumerable<object> LoadRecursiveAsync(string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in reader.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                var mapFile = PathEx.Join(directory, "pbr.properties");

                if (reader.FileExists(mapFile)) {
                    PbrProperties texture = null;

                    try {
                        texture = await pbrReader.LoadLocalAsync(mapFile, token);
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

                var textureList = new List<PbrProperties>();
                foreach (var filename in reader.EnumerateFiles(directory, "*.pbr.properties")) {
                    textureList.Clear();

                    try {
                        var texture = await pbrReader.LoadGlobalAsync(filename, token);
                        ignoreList.Add(filename);

                        if (pbrReader.TryExpandRange(texture, out var subTextureList)) {
                            foreach (var subTexture in subTextureList) {
                                textureList.Add(subTexture);
                                ignoreList.AddRange(subTexture.GetAllTextures(reader));
                            }
                        }
                        else {
                            textureList.Add(texture);
                            ignoreList.AddRange(texture.GetAllTextures(reader));
                        }
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{mapFile}'!");
                    }

                    foreach (var texture in textureList)
                        yield return texture;
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

        private static readonly string[] IgnoredExtensions = {".zip", ".db", ".cmd", ".sh", ".xcf", ".psd"};
    }
}
