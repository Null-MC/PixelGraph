using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal static class ImageProcessors
    {
        public static void CopyRegion<TPixel>(Image<TPixel> srcImage, int srcX, int srcY, Image<TPixel> destImage, Rectangle destBounds)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            srcImage.ProcessPixelRows(destImage, (src, dest) => {
                for (var y = 0; y < destBounds.Height; y++) {
                    var srcRow = src.GetRowSpan(srcY + y);
                    var destRow = dest.GetRowSpan(destBounds.Y + y);

                    var srcSlice = srcRow.Slice(srcX, destBounds.Width);
                    var destSlice = destRow.Slice(destBounds.X, destBounds.Width);
                    
                    srcSlice.CopyTo(destSlice);
                }
            });
        }

        public static float GetMaxValue(Image srcImage, ColorChannel color)
        {
            var rowMaxValues = new float[srcImage.Height];

            srcImage.Mutate(c => c.ProcessPixelRowsAsVector4((row, pos) => {
                var rowMax = 0.0f;
                for (var x = 0; x < srcImage.Width; x++) {
                    row[x].GetChannelValue(in color, out var value);
                    
                    if (x == 0) rowMax = value;
                    else if (value > rowMax) rowMax = value;
                }

                rowMaxValues[pos.Y] = rowMax;
            }));

            return rowMaxValues.Max();
        }

        public static void Shift(Image srcImage, ColorChannel color, float offset)
        {
            srcImage.Mutate(c => c.ProcessPixelRowsAsVector4(row => {
                for (var x = 0; x < srcImage.Width; x++) {
                    row[x].GetChannelValue(in color, out var value);

                    value = MathEx.Saturate(value + offset);
                    
                    row[x].SetChannelValue(in color, in value);
                }
            }));
        }

        public static void ExtractMagnitude<TSrcPixel, TDestPixel>(Image<TSrcPixel> srcImage, Image<TDestPixel> destImage, PixelMapping mapping, Rectangle bounds)
            where TSrcPixel : unmanaged, IPixel<TSrcPixel>
            where TDestPixel : unmanaged, IPixel<TDestPixel>
        {
            srcImage.ProcessPixelRows(destImage, (src, dest) => {
                var pixelMag = new Rgba32();
                Vector3 normal;

                for (var y = 0; y < bounds.Height; y++) {
                    var srcRow = src.GetRowSpan(bounds.Y + y);
                    var destRow = dest.GetRowSpan(bounds.Y + y);

                    var srcSlice = srcRow.Slice(bounds.X, bounds.Width);
                    var destSlice = destRow.Slice(bounds.X, bounds.Width);

                    for (var x = 0; x < bounds.Width; x++) {
                        var normalPixel = srcSlice[x].ToScaledVector4();
                        normal.X = normalPixel.X * 2f - 1f;
                        normal.Y = normalPixel.Y * 2f - 1f;
                        normal.Z = normalPixel.Z * 2f - 1f;

                        var magnitude = normal.Length();
                        if (!mapping.TryUnmap(in magnitude, out var value)) continue;

                        pixelMag.SetChannelValueScaled(ColorChannel.Red, in value);
                        pixelMag.SetChannelValueScaled(ColorChannel.Green, in value);
                        pixelMag.SetChannelValueScaled(ColorChannel.Blue, in value);
                        destSlice[x].FromRgba32(pixelMag);
                    }
                }
            });
        }
    }
}
