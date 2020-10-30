using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace McPbrPipeline.Internal.Input
{
    internal abstract class BaseInputReader : IInputReader
    {
        private readonly INamingStructure naming;


        protected BaseInputReader(INamingStructure naming)
        {
            this.naming = naming;
        }

        public abstract void SetRoot(string absolutePath);
        public abstract IEnumerable<string> EnumerateDirectories(string localPath, string pattern);
        public abstract IEnumerable<string> EnumerateFiles(string localPath, string pattern);
        public abstract bool FileExists(string localFile);
        public abstract Stream Open(string localFile);
        public abstract DateTime? GetWriteTime(string localFile);

        public IEnumerable<string> EnumerateTextures(PbrProperties texture, string tag)
        {
            var filename = TextureTags.Get(texture, tag);

            while (filename != null) {
                var linkedFilename = TextureTags.Get(texture, filename);
                if (string.IsNullOrEmpty(linkedFilename)) break;

                tag = filename;
                filename = linkedFilename;
            }

            var srcPath = texture.UseGlobalMatching
                ? texture.Path : PathEx.Join(texture.Path, texture.Name);

            if (!string.IsNullOrEmpty(filename))
                yield return PathEx.Join(srcPath, filename);

            var matchName = naming.GetInputTextureName(tag, texture.Name, texture.UseGlobalMatching);

            foreach (var file in EnumerateFiles(srcPath, matchName)) {
                var ext = Path.GetExtension(file);

                if (ImageExtensions.Supported.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
                    yield return file;
            }
        }
    }
}
