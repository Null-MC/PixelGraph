using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PixelGraph.Common.IO
{
    public interface IFileLoader
    {
        bool Expand {get; set;}

        IAsyncEnumerable<object> LoadAsync(CancellationToken token = default);
    }

    internal class FileLoader : IFileLoader
    {
        private readonly IInputReader reader;
        private readonly IMaterialReader pbrReader;
        private readonly ILogger logger;
        private readonly Stack<string> untracked;
        private readonly HashSet<string> ignored;

        public bool Expand {get; set;} = true;


        public FileLoader(
            IInputReader reader,
            IMaterialReader pbrReader,
            ILogger<FileLoader> logger)
        {
            this.reader = reader;
            this.pbrReader = pbrReader;
            this.logger = logger;

            untracked = new Stack<string>();
            ignored = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public async IAsyncEnumerable<object> LoadAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            untracked.Clear();
            ignored.Clear();

            await foreach (var texture in LoadRecursiveAsync(".", token))
                yield return texture;

            while (untracked.TryPop(out var filename)) {
                token.ThrowIfCancellationRequested();
                if (ignored.Contains(filename)) continue;
                yield return filename;
            }
        }

        private async IAsyncEnumerable<object> LoadRecursiveAsync(string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var directory in reader.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                if (directory.EndsWith(".ignore", StringComparison.InvariantCultureIgnoreCase)) continue;

                var localMapFile = PathEx.Join(directory, "pbr.yml");

                if (reader.FileExists(localMapFile)) {
                    MaterialProperties material = null;

                    try {
                        material = await pbrReader.LoadLocalAsync(localMapFile, token);
                        ignored.Add(localMapFile);

                        foreach (var texture in reader.EnumerateAllTextures(material))
                            ignored.Add(texture);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{localMapFile}'!");
                    }

                    if (material != null)
                        yield return material;

                    continue;
                }

                await foreach (var texture in LoadRecursiveAsync(directory, token))
                    yield return texture;

                var materialList = new List<MaterialProperties>();
                foreach (var filename in reader.EnumerateFiles(directory, "*.pbr.yml")) {
                    materialList.Clear();

                    try {
                        var material = await pbrReader.LoadGlobalAsync(filename, token);
                        ignored.Add(filename);

                        if (Expand && pbrReader.TryExpandRange(material, out var subTextureList)) {
                            foreach (var childMaterial in subTextureList) {
                                materialList.Add(childMaterial);

                                foreach (var texture in reader.EnumerateAllTextures(childMaterial))
                                    ignored.Add(texture);
                            }
                        }
                        else {
                            materialList.Add(material);

                            foreach (var texture in reader.EnumerateAllTextures(material))
                                ignored.Add(texture);
                        }
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, $"Failed to load local texture map '{localMapFile}'!");
                    }

                    foreach (var texture in materialList)
                        yield return texture;
                }

                foreach (var filename in reader.EnumerateFiles(directory, "*")) {
                    if (IgnoredExtensions.Any(x => filename.EndsWith(x, StringComparison.InvariantCultureIgnoreCase))) {
                        logger.LogDebug($"Ignoring file '{filename}'.");
                        continue;
                    }

                    untracked.Push(filename);
                }
            }
        }

        private static readonly string[] IgnoredExtensions = {".pack.yml", ".pbr.yml", ".zip", ".db", ".cmd", ".sh", ".xcf", ".psd", ".bak"};
    }
}
