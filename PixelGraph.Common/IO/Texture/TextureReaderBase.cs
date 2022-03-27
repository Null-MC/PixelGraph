using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelGraph.Common.IO.Texture
{
    public interface ITextureReader
    {
        bool TryGetSourceFilename(string tag, out string filename);

        IEnumerable<string> EnumerateInputTextures(MaterialProperties material, string tag);
        IEnumerable<string> EnumerateOutputTextures(string destName, string destPath, string tag, bool global);
        IEnumerable<string> EnumerateAllTextures(MaterialProperties material);

        bool IsLocalFile(string localFile, string tag);
        bool IsGlobalFile(string localFile, string name, string tag);
    }

    internal abstract class TextureReaderBase : ITextureReader
    {
        protected ITextureGraphContext Context {get;}
        protected IInputReader Reader {get;}


        protected TextureReaderBase(IServiceProvider provider)
        {
            Context = provider.GetRequiredService<ITextureGraphContext>();
            Reader = provider.GetRequiredService<IInputReader>();
        }

        public virtual bool TryGetSourceFilename(string tag, out string filename)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var textureList = Context.IsImport
                ? EnumerateOutputTextures(Context.Material.Name, Context.Material.LocalPath, tag, true)
                : EnumerateInputTextures(Context.Material, tag);

            foreach (var file in textureList) {
                // TODO: All enum files should exist, why are we checking?
                if (!Reader.FileExists(file)) continue;

                filename = file;
                return true;
            }

            filename = null;
            return false;
        }

        public virtual IEnumerable<string> EnumerateInputTextures(MaterialProperties material, string tag)
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

            foreach (var file in Reader.EnumerateFiles(srcPath)) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var isMatch = material.UseGlobalMatching
                    ? IsGlobalFile(file, material.Name, tag)
                    : IsLocalFile(file, tag);

                if (isMatch) yield return file;
            }
        }

        public virtual IEnumerable<string> EnumerateOutputTextures(string destName, string destPath, string tag, bool global)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var srcPath = global
                ? destPath : PathEx.Join(destPath, destName);

            foreach (var file in Reader.EnumerateFiles(srcPath)) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var isMatch = global
                    ? IsGlobalFile(file, destName, tag)
                    : IsLocalFile(file, tag);

                if (isMatch) yield return file;
            }
        }

        public abstract bool IsLocalFile(string localFile, string tag);
        //{
        //    return NamingStructure.IsLocalFileTag(localFile, tag);
        //}

        public abstract bool IsGlobalFile(string localFile, string name, string tag);
        //{
        //    return NamingStructure.IsGlobalFileTag(localFile, name, tag);
        //}

        public virtual IEnumerable<string> EnumerateAllTextures(MaterialProperties material)
        {
            return TextureTags.All
                .SelectMany(tag => EnumerateInputTextures(material, tag))
                .Where(file => file != null).Distinct();
        }
    }
}
