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
        public abstract IEnumerable<string> EnumerateDirectories(string localPath, string pattern);
        public abstract IEnumerable<string> EnumerateFiles(string localPath, string pattern);
        public abstract bool FileExists(string localFile);
        public abstract string GetFullPath(string localFile);
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

            var matchName = NamingStructure.GetInputTextureName(material, tag);

            foreach (var file in FilesMatching(srcPath, matchName))
                yield return file;
        }

        public IEnumerable<string> EnumerateOutputTextures(ResourcePackProfileProperties pack, string destName, string destPath, string tag, bool global)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var srcPath = global
                ? destPath : PathEx.Join(destPath, destName);

            var ext = NamingStructure.GetExtension(pack);
            var tagFileName = NamingStructure.Get(tag, destName, ext, true);
            return FilesMatching(srcPath, tagFileName);
        }

        public IEnumerable<string> EnumerateAllTextures(MaterialProperties material)
        {
            return TextureTags.All
                .SelectMany(tag => EnumerateInputTextures(material, tag))
                .Where(file => file != null).Distinct();
        }

        private IEnumerable<string> FilesMatching(string srcPath, string matchName)
        {
            foreach (var file in EnumerateFiles(srcPath, matchName)) {
                var ext = Path.GetExtension(file);

                if (ImageExtensions.Supports(ext))
                    yield return file;
            }
        }
    }
}
