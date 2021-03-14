using SixLabors.ImageSharp;
using System;

namespace PixelGraph.Common.Extensions
{
    public static class RectangleExtensions
    {
        public static Rectangle ScaleTo(this in RectangleF rectangle, in int width, in int height)
        {
            return new Rectangle {
                X = (int)MathF.Ceiling(rectangle.X * width),
                Y = (int)MathF.Ceiling(rectangle.Y * height),
                Width = (int)MathF.Ceiling(rectangle.Width * width),
                Height = (int)MathF.Ceiling(rectangle.Height * height),
            };
        }
    }
}
