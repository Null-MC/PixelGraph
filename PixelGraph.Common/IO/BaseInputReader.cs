using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelGraph.Common.IO
{
    public abstract class BaseInputReader : IInputReader
    {
        private readonly INamingStructure naming;


        protected BaseInputReader(INamingStructure naming)
        {
            this.naming = naming;
        }

        public abstract void SetRoot(string absolutePath);
        public abstract string GetFullPath(string localPath);
        public abstract IEnumerable<string> EnumerateDirectories(string localPath, string pattern);
        public abstract IEnumerable<string> EnumerateFiles(string localPath, string pattern);
        public abstract bool FileExists(string localFile);
        public abstract Stream Open(string localFile);
        public abstract DateTime? GetWriteTime(string localFile);

        public IEnumerable<string> EnumerateTextures(MaterialProperties material, string tag)
        {
            var srcPath = material.UseGlobalMatching
                ? material.LocalPath : PathEx.Join(material.LocalPath, material.Name);

            var localName = TextureTags.Get(material, tag);

            while (localName != null) {
                //var localFile = PathEx.Join(srcPath, localName);

                //if (FileExists(localFile)) {
                //    yield return localFile;
                //    yield break;
                //}

                var linkedFilename = TextureTags.Get(material, localName);
                if (string.IsNullOrEmpty(linkedFilename)) break;

                tag = localName;
                localName = linkedFilename;
            }

            if (!string.IsNullOrEmpty(localName)) {
                localName = PathEx.Normalize(localName);
                yield return PathEx.Join(srcPath, localName);
            }

            var matchName = naming.GetInputTextureName(material, tag);

            foreach (var file in EnumerateFiles(srcPath, matchName)) {
                var ext = Path.GetExtension(file);

                if (ImageExtensions.Supported.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
                    yield return file;
            }
        }

        public IEnumerable<string> EnumerateAllTextures(MaterialProperties material)
        {
            return TextureTags.All
                .SelectMany(tag => EnumerateTextures(material, tag))
                .Where(file => file != null).Distinct();
        }
    }
}
