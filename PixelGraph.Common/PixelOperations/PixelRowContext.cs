using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;

namespace PixelGraph.Common.PixelOperations
{
    public readonly struct PixelRowContext
    {
        public readonly Rectangle Bounds;
        public readonly int Y;


        public PixelRowContext(in Rectangle bounds, in int y)
        {
            Bounds = bounds;
            Y = y;
        }

        public void WrapX(ref int x)
        {
            TexCoordHelper.WrapCoordX(ref x, in Bounds);
        }

        public void WrapY(ref int y)
        {
            TexCoordHelper.ClampCoordY(ref y, in Bounds);
        }

        public void Wrap(ref int x, ref int y)
        {
            WrapX(ref x);
            WrapY(ref y);
        }

        public void ClampX(ref int x)
        {
            TexCoordHelper.ClampCoordX(ref x, in Bounds);
        }

        public void ClampY(ref int y)
        {
            TexCoordHelper.ClampCoordY(ref y, in Bounds);
        }

        public void Clamp(ref int x, ref int y)
        {
            ClampX(ref x);
            ClampY(ref y);
        }
    }
}
