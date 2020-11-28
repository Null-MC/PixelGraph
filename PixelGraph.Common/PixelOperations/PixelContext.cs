using System;

namespace PixelGraph.Common.PixelOperations
{
    public struct PixelContext
    {
        private readonly Lazy<byte[]> noiseX;
        private readonly Lazy<byte[]> noiseY;

        public readonly int Width;
        public readonly int Height;
        public readonly int Y;
        public int X;


        public PixelContext(in int width, in int height, in int y)
        {
            Width = width;
            Height = height;
            Y = y;
            X = 0;

            var _width = width;
            noiseX = new Lazy<byte[]>(() => {
                GenerateNoise(in _width, out var buffer);
                return buffer;
            });
            noiseY = new Lazy<byte[]>(() => {
                GenerateNoise(in _width, out var buffer);
                return buffer;
            });
        }

        public readonly void Wrap(ref int x, ref int y)
        {
            while (x < 0) x += Width;
            while (y < 0) y += Height;

            while (x >= Width) x -= Width;
            while (y >= Height) y -= Height;
        }

        public readonly void Clamp(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x >= Width) x = Width - 1;
            if (y >= Height) y = Height - 1;
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
