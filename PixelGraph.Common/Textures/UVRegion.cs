using SixLabors.ImageSharp;
using System;

namespace PixelGraph.Common.Textures;

public struct UVRegion
{
    public double Left;
    public double Top;
    public double Right;
    public double Bottom;
    //public double Width;
    //public double Height;

    public double Width => Right - Left;
    public double Height => Bottom - Top;


    public UVRegion(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public readonly void ScaleTo(in int width, in int height, ref Rectangle region)
    {
        region.X = (int)Math.Round(Left * width);
        region.Y = (int)Math.Round(Top * height);
        region.Width = (int)Math.Round((Right - Left) * width);
        region.Height = (int)Math.Round((Bottom - Top) * height);
    }

    public readonly Rectangle ScaleTo(in int width, in int height)
    {
        var region = new Rectangle();
        ScaleTo(in width, in height, ref region);
        return region;
    }

    //public readonly void GetTexCoord(in double x, in double y, out double fx, out double fy)
    //{
    //    fx = Left * (1d - x) + Right * x;
    //    fy = Top * (1d - y) + Bottom * y;
    //}

    public static readonly UVRegion Empty = new();
    public static readonly UVRegion Full = new(0f, 0f, 1f, 1f);
}