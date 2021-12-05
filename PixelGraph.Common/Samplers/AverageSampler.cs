using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class AverageSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float x, in float y, ref Rgba32 pixel)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);
            
            var minRangeX = MathF.Max(RangeX, 1f);
            var minRangeY = MathF.Max(RangeY, 1f);
            
            var stepX = (int)MathF.Ceiling(minRangeX);
            var stepY = (int)MathF.Ceiling(minRangeY);

            var pxMin = (int)(fx + 0.25f);
            var pyMin = (int)(fy + 0.25f);
            var pxMax = pxMin + stepX;
            var pyMax = pyMin + stepY;

            var color = new Vector4();
            for (var py = pyMin; py < pyMax; py++) {
                var _py = py;

                if (WrapY) WrapCoordY(ref _py);
                else ClampCoordY(ref _py);

                for (var px = pxMin; px < pxMax; px++) {
                    var _px = px;

                    if (WrapX) WrapCoordX(ref _px);
                    else ClampCoordX(ref _px);

                    color += Image[_px, _py].ToScaledVector4();
                }
            }

            var h = stepX * stepY;
            pixel.FromScaledVector4(color / h);
        }
    }
}
