using SixLabors.ImageSharp;
using System;

namespace PixelGraph.Common.Extensions
{
    public static class RectangleExtensions
    {
        public static Rectangle ScaleTo(this in RectangleF rectangle, in int width, in int height)
        {
            return new Rectangle {
                X = (int)MathF.Floor(rectangle.X * width + 0.25f),
                Y = (int)MathF.Floor(rectangle.Y * height + 0.25f),
                Width = (int)MathF.Floor(rectangle.Width * width + 0.25f),
                Height = (int)MathF.Floor(rectangle.Height * height + 0.25f),
            };
        }
    }
}
