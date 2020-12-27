using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.PixelOperations
{
    internal delegate void PixelRowAction<TPixel>(in PixelRowContext context, Span<TPixel> row);

    internal delegate void PixelAction(ref Rgba32 value, in PixelContext context);
}
