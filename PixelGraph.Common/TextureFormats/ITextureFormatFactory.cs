using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.TextureFormats
{
    public interface ITextureFormatFactory
    {
        PackEncoding Create();
    }
}
