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

        public readonly void Wrap(ref int x, ref int y)
        {
            while (x < Bounds.Left) x += Bounds.Width;
            while (y < Bounds.Top) y += Bounds.Height;

            while (x >= Bounds.Right) x -= Bounds.Width;
            while (y >= Bounds.Bottom) y -= Bounds.Height;
        }

        public readonly void Clamp(ref int x, ref int y)
        {
            if (x < Bounds.Left) x = Bounds.Left;
            if (y < Bounds.Top) y = Bounds.Top;

            if (x >= Bounds.Right) x = Bounds.Right - 1;
            if (y >= Bounds.Bottom) y = Bounds.Bottom - 1;
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
