using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;

namespace PixelGraph.Common.Extensions;

public static class PixelExtensions
{
    private static readonly Type[] grayscalePixelTypes = {typeof(L8), typeof(L16), typeof(La16), typeof(La32)};


    public static bool IsGrayscale<TPixel>(this Image<TPixel> _)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        return grayscalePixelTypes.Contains(typeof(TPixel));
    }
}