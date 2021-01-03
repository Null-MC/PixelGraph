using SixLabors.ImageSharp;

namespace PixelGraph.Common.PixelOperations
{
    public struct PixelRowContext
    {
        public readonly Rectangle Bounds;
        public readonly int Y;


        public PixelRowContext(in Rectangle bounds, in int y)
        {
            Bounds = bounds;
            Y = y;
        }
    }
}
