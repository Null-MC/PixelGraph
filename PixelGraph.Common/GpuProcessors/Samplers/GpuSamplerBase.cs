using System;

namespace PixelGraph.Common.GpuProcessors.Samplers
{
    internal static class GpuSampler
    {
        public static void WrapClampX(this IGpuSampler sampler, ref int x)
        {
            if (sampler.WrapX) sampler.WrapX(ref x);
            else sampler.ClampX(ref x);
        }

        public static void WrapClampY(this IGpuSampler sampler, ref int y)
        {
            if (sampler.WrapY) sampler.WrapY(ref y);
            else sampler.ClampY(ref y);
        }

        public static void WrapX(this IGpuSampler sampler, ref int x)
        {
            if (sampler.BufferWidth == 0) throw new ArgumentOutOfRangeException(nameof(sampler.BufferWidth));

            while (x < 0) x += sampler.BufferWidth;
            while (x >= sampler.BufferWidth) x -= sampler.BufferWidth;
        }

        public static void WrapY(this IGpuSampler sampler, ref int y)
        {
            if (sampler.BufferHeight == 0) throw new ArgumentOutOfRangeException(nameof(sampler.BufferHeight));

            while (y < 0) y += sampler.BufferHeight;
            while (y >= sampler.BufferHeight) y -= sampler.BufferHeight;
        }

        public static void ClampX(this IGpuSampler sampler, ref int x)
        {
            if (sampler.BufferWidth == 0) throw new ArgumentOutOfRangeException(nameof(sampler.BufferWidth));

            if (x < 0) x = sampler.BufferWidth;
            if (x >= sampler.BufferWidth) x = sampler.BufferWidth - 1;
        }

        public static void ClampY(this IGpuSampler sampler, ref int y)
        {
            if (sampler.BufferHeight == 0) throw new ArgumentOutOfRangeException(nameof(sampler.BufferHeight));

            if (y < 0) y = sampler.BufferHeight;
            if (y >= sampler.BufferHeight) y = sampler.BufferHeight - 1;
        }
    }

    internal interface IGpuSampler
    {
        int BufferWidth {get;}
        int BufferHeight {get;}
        bool WrapX {get; set;}
        bool WrapY {get; set;}
    }

    internal interface IGpuSampler<T> : IGpuSampler
    {
        void Sample(in float x, in float y, out T value);
    }
}
