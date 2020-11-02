using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.PixelOperations
{
    internal delegate void PixelAction(ref Rgba32 value, in PixelContext context);
}
