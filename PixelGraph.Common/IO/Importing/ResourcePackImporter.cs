using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Importing
{
    public interface IResourcePackImporter
    {
        bool AsGlobal {get; set;}
        bool CopyUntracked {get; set;}
        ResourcePackInputProperties PackInput {get; set;}
        ResourcePackProfileProperties PackProfile {get; set;}

        Task ImportAsync(CancellationToken token = default);
    }

    internal class ResourcePackImporter : IResourcePackImporter
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;

        public bool AsGlobal {get; set;}
        public bool CopyUntracked {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}
        public ResourcePackProfileProperties PackProfile {get; set;}


        public ResourcePackImporter(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            writer = provider.GetRequiredService<IOutputWriter>();
        }

        public async Task ImportAsync(CancellationToken token = default)
        {
            await ImportPathAsync(".", token);
        }

        private async Task ImportPathAsync(string localPath, CancellationToken token)
        {
            foreach (var childPath in reader.EnumerateDirectories(localPath, "*"))
                await ImportPathAsync(childPath, token);

            var files = reader.EnumerateFiles(localPath, "*.*")
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            var names = GetMaterialNames(files).Distinct().ToArray();

            foreach (var name in names) {
                token.ThrowIfCancellationRequested();

                await ImportMaterialAsync(localPath, name, token);

                // Remove from untracked files
                _ = ExtractTextureFile(files, name);
                _ = ExtractTextureFile(files, $"{name}_n");
                _ = ExtractTextureFile(files, $"{name}_s");
                _ = ExtractTextureFile(files, $"{name}_e");
            }

            if (!CopyUntracked) return;

            foreach (var file in files) {
                token.ThrowIfCancellationRequested();

                await CopyFileAsync(file, token);
            }
        }

        private async Task ImportMaterialAsync(string localPath, string name, CancellationToken token)
        {
            var importer = provider.GetRequiredService<IMaterialImporter>();

            importer.AsGlobal = AsGlobal;
            importer.LocalPath = localPath;
            importer.PackInput = PackInput;
            importer.PackProfile = PackProfile;
            await importer.ImportAsync(name, token);
        }

        private async Task CopyFileAsync(string file, CancellationToken token)
        {
            await using var sourceStream = reader.Open(file);
            await using var destStream = writer.Open(file);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private static string ExtractTextureFile(ICollection<string> files, string name)
        {
            var file = files.FirstOrDefault(f => {
                var fName = Path.GetFileNameWithoutExtension(f);
                return string.Equals(fName, name, StringComparison.InvariantCultureIgnoreCase);
            });

            if (file != null)
                files.Remove(file);

            return file;
        }

        private static IEnumerable<string> GetMaterialNames(IEnumerable<string> files)
        {
            foreach (var file in files) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var name = Path.GetFileNameWithoutExtension(file);

                var isNormal = name.EndsWith("_n", StringComparison.InvariantCultureIgnoreCase);
                var isSpecular = name.EndsWith("_s", StringComparison.InvariantCultureIgnoreCase);
                var isEmissive = name.EndsWith("_e", StringComparison.InvariantCultureIgnoreCase);

                if (isNormal || isSpecular || isEmissive)
                    yield return name[..^2];
            }
        }
    }
}
