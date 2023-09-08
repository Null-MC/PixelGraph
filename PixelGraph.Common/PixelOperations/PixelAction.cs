using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.PixelOperations;

internal delegate void PixelRowAction<TPixel>(in PixelRowContext context, Span<TPixel> row)
    where TPixel : unmanaged, IPixel<TPixel>;

//internal delegate void PixelAction<TPixel>(ref TPixel value, in PixelContext context)
//    where TPixel : unmanaged, IPixel<TPixel>;

//internal delegate void PixelAction(ref Rgba32 value, in PixelContext context);