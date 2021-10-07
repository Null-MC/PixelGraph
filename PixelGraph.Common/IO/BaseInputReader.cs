using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelGraph.Common.IO
{
    public abstract class BaseInputReader : IInputReader
    {
        public abstract void SetRoot(string absolutePath);
        public abstract IEnumerable<string> EnumerateDirectories(string localPath, string pattern = null);
        public abstract IEnumerable<string> EnumerateFiles(string localPath, string pattern = null);
        public abstract bool FileExists(string localFile);
        public abstract string GetFullPath(string localFile);
        public abstract string GetRelativePath(string fullPath);
        public abstract Stream Open(string localFile);
        public abstract DateTime? GetWriteTime(string localFile);

        public IEnumerable<string> EnumerateInputTextures(MaterialProperties material, string tag)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var srcPath = material.UseGlobalMatching
                ? material.LocalPath : PathEx.Join(material.LocalPath, material.Name);

            var localName = TextureTags.Get(material, tag);

            while (localName != null) {
                var linkedFilename = TextureTags.Get(material, localName);
                if (string.IsNullOrEmpty(linkedFilename)) break;

                tag = localName;
                localName = linkedFilename;
            }

            if (!string.IsNullOrEmpty(localName)) {
                localName = PathEx.Localize(localName);
                yield return PathEx.Join(srcPath, localName);
            }

            foreach (var file in EnumerateFiles(srcPath)) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var isMatch = material.UseGlobalMatching
                    ? NamingStructure.IsGlobalFileTag(file, material.Name, tag)
                    : NamingStructure.IsLocalFileTag(file, tag);

                if (isMatch) yield return file;
            }
        }

        public IEnumerable<string> EnumerateOutputTextures(ResourcePackProfileProperties pack, string destName, string destPath, string tag, bool global)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var srcPath = global
                ? destPath : PathEx.Join(destPath, destName);

            foreach (var file in EnumerateFiles(srcPath)) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var isMatch = global
                    ? NamingStructure.IsGlobalFileTag(file, destName, tag)
                    : NamingStructure.IsLocalFileTag(file, tag);

                if (isMatch) yield return file;
            }
        }

        public IEnumerable<string> EnumerateAllTextures(MaterialProperties material)
        {
            return TextureTags.All
                .SelectMany(tag => EnumerateInputTextures(material, tag))
                .Where(file => file != null).Distinct();
        }
    }
}
