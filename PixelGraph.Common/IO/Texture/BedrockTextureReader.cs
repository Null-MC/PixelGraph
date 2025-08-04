using PixelGraph.Common.Textures;
using System;
using System.IO;

namespace PixelGraph.Common.IO.Texture;

internal class BedrockTextureReader : TextureReaderBase
{
    public BedrockTextureReader(IServiceProvider provider) : base(provider) {}

    public override bool IsLocalFile(string localFile, string tag)
    {
        var fileName = Path.GetFileNameWithoutExtension(localFile);

        if (TextureTags.Is(tag, TextureTags.Color))
            return string.Equals(fileName, "basecolor", StringComparison.InvariantCultureIgnoreCase);

        if (TextureTags.Is(tag, TextureTags.Height))
            return string.Equals(fileName, "heightmap", StringComparison.InvariantCultureIgnoreCase);

        if (TextureTags.Is(tag, TextureTags.Normal))
            return string.Equals(fileName, "normal", StringComparison.InvariantCultureIgnoreCase);
            
        if (TextureTags.Is(tag, TextureTags.MER))
            return string.Equals(fileName, "mer", StringComparison.InvariantCultureIgnoreCase);
            
        if (TextureTags.Is(tag, TextureTags.MERS))
            return string.Equals(fileName, "mers", StringComparison.InvariantCultureIgnoreCase);

        return false;
    }

    public override bool IsGlobalFile(string localFile, string name, string tag)
    {
        var fileName = Path.GetFileNameWithoutExtension(localFile);

        if (TextureTags.Is(tag, TextureTags.Color))
            return string.Equals(fileName, name, StringComparison.InvariantCultureIgnoreCase);

        if (TextureTags.Is(tag, TextureTags.Normal))
            return string.Equals(fileName, $"{name}_normal", StringComparison.InvariantCultureIgnoreCase);

        if (TextureTags.Is(tag, TextureTags.Height))
            return string.Equals(fileName, $"{name}_heightmap", StringComparison.InvariantCultureIgnoreCase);
            
        if (TextureTags.Is(tag, TextureTags.MER))
            return string.Equals(fileName, $"{name}_mer", StringComparison.InvariantCultureIgnoreCase);
            
        if (TextureTags.Is(tag, TextureTags.MERS))
            return string.Equals(fileName, $"{name}_mers", StringComparison.InvariantCultureIgnoreCase);

        return false;
    }
}