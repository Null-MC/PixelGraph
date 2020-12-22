using SixLabors.ImageSharp;
using System;

namespace PixelGraph.Common.PixelOperations
{
    public struct PixelContext
    {
        private readonly Lazy<byte[]> noiseX;
        private readonly Lazy<byte[]> noiseY;

        public readonly Rectangle Bounds;
        public readonly int Y;
        public int X;


        public PixelContext(in Rectangle bounds, in int y)
        {
            Bounds = bounds;
            Y = y;
            X = 0;

            var _width = bounds.Width;
            noiseX = new Lazy<byte[]>(() => {
                GenerateNoise(in _width, out var buffer);
                return buffer;
            });

            var _height = bounds.Height;
            noiseY = new Lazy<byte[]>(() => {
                GenerateNoise(in _height, out var buffer);
                return buffer;
            });
        }

        public readonly void WrapX(ref int x)
        {
            while (x < Bounds.Left) x += Bounds.Width;
            while (x >= Bounds.Right) x -= Bounds.Width;
        }

        public readonly void WrapY(ref int y)
        {
            while (y < Bounds.Top) y += Bounds.Height;
            while (y >= Bounds.Bottom) y -= Bounds.Height;
        }

        public readonly void Wrap(ref int x, ref int y)
        {
            WrapX(ref x);
            WrapY(ref y);
        }

        public readonly void ClampX(ref int x)
        {
            if (x < Bounds.Left) x = Bounds.Left;
            if (x >= Bounds.Right) x = Bounds.Right - 1;
        }

        public readonly void ClampY(ref int y)
        {
            if (y < Bounds.Top) y = Bounds.Top;
            if (y >= Bounds.Bottom) y = Bounds.Bottom - 1;
        }

        public readonly void Clamp(ref int x, ref int y)
        {
            ClampX(ref x);
            ClampY(ref y);
        }

        public readonly void GetNoise(in int x, out byte valueX, out byte valueY)
        {
            valueX = noiseX.Value[x];
            valueY = noiseY.Value[x];
        }

        private static void GenerateNoise(in int size, out byte[] buffer)
        {
            var random = new Random();
            buffer = new byte[size];
            random.NextBytes(buffer);
        }
    }
}
