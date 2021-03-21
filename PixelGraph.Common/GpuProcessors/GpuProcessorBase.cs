using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PixelGraph.Common.GpuProcessors
{
    internal abstract class GpuProcessorBase
    {
        protected const float HalfPixel = 0.5f - float.Epsilon;


        protected static float[] GetFloatBuffer<TPixel>(Image<TPixel> image, ColorChannel color)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            int width = image.Width, height = image.Height;
            var vectorArray = new float[height * width];

            Parallel.For(0, height, i => {
                ref var rPixel = ref image.GetPixelRowSpan(i).GetPinnableReference();
                ref var r4 = ref vectorArray[i * width];

                for (var j = 0; j < width; j++) {
                    var v4 = Unsafe.Add(ref rPixel, j).ToVector4();
                    v4.GetChannelValue(in color, out var value);
                    Unsafe.Add(ref r4, j) = value;
                }
            });

            return vectorArray;
        }

        protected void FillImage<TPixel>(Image<TPixel> image, float[] buffer)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            int width = image.Width, height = image.Height;

            Parallel.For(0, height, i => {
                ref var rPixel = ref image.GetPixelRowSpan(i).GetPinnableReference();
                ref var r4 = ref buffer[i * width];

                for (var j = 0; j < width; j++) {
                    var value = Unsafe.Add(ref r4, j);
                    var v4 = new Vector4(value);

                    Unsafe.Add(ref rPixel, j).FromScaledVector4(v4);
                }
            });
        }
    }
}
