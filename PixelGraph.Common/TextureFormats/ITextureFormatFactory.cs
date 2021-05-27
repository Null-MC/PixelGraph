using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.TextureFormats
{
    public interface ITextureFormatFactory
    {
        ResourcePackEncoding Create();
    }
}
