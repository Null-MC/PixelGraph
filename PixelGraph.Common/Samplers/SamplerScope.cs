using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Samplers
{
    internal class SamplerScope<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Dictionary<int, Memory<TPixel>> rows;


    }
}
