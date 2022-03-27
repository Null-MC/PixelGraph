using PixelGraph.Common.Textures;
using System;
using System.IO;

namespace PixelGraph.Common.IO.Texture
{
    internal class JavaTextureReader : TextureReaderBase
    {
        public JavaTextureReader(IServiceProvider provider) : base(provider) {}

        public override bool IsLocalFile(string localFile, string tag)
        {
            var fileName = Path.GetFileNameWithoutExtension(localFile);

            if (TextureTags.Is(tag, TextureTags.Color))
                return string.Equals(fileName, "basecolor", StringComparison.InvariantCultureIgnoreCase);

            if (TextureTags.Is(tag, TextureTags.Normal))
                return string.Equals(fileName, "normal", StringComparison.InvariantCultureIgnoreCase);
            
            if (TextureTags.Is(tag, TextureTags.Specular))
                return string.Equals(fileName, "specular", StringComparison.InvariantCultureIgnoreCase);

            return false;
        }

        public override bool IsGlobalFile(string localFile, string name, string tag)
        {
            var fileName = Path.GetFileNameWithoutExtension(localFile);

            if (TextureTags.Is(tag, TextureTags.Color))
                return string.Equals(fileName, name, StringComparison.InvariantCultureIgnoreCase);

            if (TextureTags.Is(tag, TextureTags.Normal))
                return string.Equals(fileName, $"{name}_n", StringComparison.InvariantCultureIgnoreCase);
            
            if (TextureTags.Is(tag, TextureTags.Specular))
                return string.Equals(fileName, $"{name}_s", StringComparison.InvariantCultureIgnoreCase);

            return false;
        }
    }
}
