using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
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
        bool EnableAutoMaterial {get; set;}

        IAsyncEnumerable<object> LoadAsync(CancellationToken token = default);
        bool IsLocalMaterialPath(string localPath);
    }

    internal class FileLoader : IFileLoader
    {
        private readonly IInputReader reader;
        private readonly IMaterialReader materialReader;
        private readonly ILogger logger;
        private readonly Stack<string> untracked;
        private readonly HashSet<string> ignored;

        public bool EnableAutoMaterial {get; set;}


        public FileLoader(
            IInputReader reader,
            IMaterialReader materialReader,
            ILogger<FileLoader> logger)
        {
            this.reader = reader;
            this.materialReader = materialReader;
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

                var fullFile = reader.GetFullPath(filename);
                if (ignored.Contains(fullFile)) continue;

                yield return filename;
            }
        }

        private async IAsyncEnumerable<object> LoadRecursiveAsync(string searchPath, [EnumeratorCancellation] CancellationToken token)
        {
            if (searchPath.EndsWith(".ignore", StringComparison.InvariantCultureIgnoreCase))
                yield break;

            if (TryFindLocalFile(searchPath, out var localMapFile)) {
                MaterialProperties material = null;

                try {
                    material = await materialReader.LoadLocalAsync(localMapFile, token);

                    var fullMapFile = reader.GetFullPath(localMapFile);
                    ignored.Add(fullMapFile);

                    // add all input textures to ignored
                    foreach (var localTexture in reader.EnumerateAllTextures(material)) {
                        var fullTextureFile = reader.GetFullPath(localTexture);
                        ignored.Add(fullTextureFile);
                    }

                    // add properties file to ignored
                    var propsFile = PathEx.Join(material.Name, "mat.properties");
                    propsFile = PathEx.Join(material.LocalPath, propsFile);
                    var fullPropsFile = reader.GetFullPath(propsFile);
                    ignored.Add(fullPropsFile);

                    // TODO: add mcmeta files to ignored
                }
                catch (Exception error) {
                    logger.LogWarning(error, $"Failed to load local texture map '{localMapFile}'!");
                }

                if (material != null)
                    yield return material;

                yield break;
            }

            if (EnableAutoMaterial && IsLocalMaterialPath(searchPath)) {
                yield return new MaterialProperties {
                    LocalFilename = PathEx.Join(searchPath, "mat.yml"),
                    LocalPath = Path.GetDirectoryName(searchPath),
                    Name = Path.GetFileName(searchPath),
                };

                yield break;
            }

            foreach (var directory in reader.EnumerateDirectories(searchPath, "*")) {
                token.ThrowIfCancellationRequested();

                await foreach (var texture in LoadRecursiveAsync(directory, token))
                    yield return texture;
            }

            var materialList = new List<MaterialProperties>();
            foreach (var filename in reader.EnumerateFiles(searchPath, "*.*.yml")) {
                if (!filename.EndsWith(".mat.yml", StringComparison.InvariantCultureIgnoreCase)
                    && !filename.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase)) continue;

                materialList.Clear();

                try {
                    var material = await materialReader.LoadGlobalAsync(filename, token);
                    var fullFile = reader.GetFullPath(filename);
                    ignored.Add(fullFile);

                    //if (materialReader.TryExpandRange(material, out var subTextureList)) {
                    //    foreach (var childMaterial in subTextureList) {
                    //        materialList.Add(childMaterial);

                    //        foreach (var texture in reader.EnumerateAllTextures(childMaterial))
                    //            ignored.Add(texture);
                    //    }
                    //}
                    //else {
                        materialList.Add(material);

                        foreach (var texture in reader.EnumerateAllTextures(material)) {
                            var fullTextureFile = reader.GetFullPath(texture);
                            ignored.Add(fullTextureFile);
                        }
                    //}

                    // add properties file to ignored
                    var propsFile = PathEx.Join(material.LocalPath, $"{material.Name}.properties");
                    var fullPropsFile = reader.GetFullPath(propsFile);
                    ignored.Add(fullPropsFile);

                    // TODO: add mcmeta files to ignored
                }
                catch (Exception error) {
                    logger.LogWarning(error, $"Failed to load global texture map '{filename}'!");
                }

                foreach (var texture in materialList)
                    yield return texture;
            }

            foreach (var filename in reader.EnumerateFiles(searchPath, "*")) {
                if (IgnoredExtensions.Any(x => filename.EndsWith(x, StringComparison.InvariantCultureIgnoreCase))) {
                    logger.LogDebug($"Ignoring file '{filename}'.");
                    continue;
                }

                untracked.Push(filename);
            }
        }

        private bool TryFindLocalFile(string searchPath, out string filename)
        {
            filename = PathEx.Join(searchPath, "mat.yml");
            if (reader.FileExists(filename)) return true;

            filename = PathEx.Join(searchPath, "pbr.yml");
            if (reader.FileExists(filename)) return true;

            return false;
        }

        public bool IsLocalMaterialPath(string localPath)
        {
            foreach (var localFile in reader.EnumerateFiles(localPath, "*.*")) {
                var ext = Path.GetExtension(localFile);
                if (IgnoredExtensions.Contains(ext, StringComparer.InvariantCultureIgnoreCase)) continue;
                if (!ImageExtensions.Supports(ext)) continue;

                var name = Path.GetFileNameWithoutExtension(localFile);
                if (AllLocalTextures.Contains(name, StringComparer.InvariantCultureIgnoreCase)) return true;
            }

            return false;
        }

        public static string[] AllLocalTextures = {"alpha", "diffuse", "albedo", "height", "occlusion", "normal", "specular", "smooth", "rough", "metal", "f0", "porosity", "sss", "emissive"};
        private static readonly string[] IgnoredExtensions = {".pack.yml", ".mat.yml", ".pbr.yml", ".zip", ".db", ".cmd", ".sh", ".xcf", ".psd", ".bak"};
    }
}
