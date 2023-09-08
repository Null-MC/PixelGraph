using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Extensions;

internal static class ColorExtensions
{
    private const float ToScaled = 1f / 255f;
    private const float ToHalfScaled = 1f / 127f;

    //public static void GetChannelValue(this in Rgb24 pixel, in ColorChannel channel, out byte value)
    //{
    //    value = channel switch {
    //        ColorChannel.Red => pixel.R,
    //        ColorChannel.Green => pixel.G,
    //        ColorChannel.Blue => pixel.B,
    //        _ => 0,
    //    };
    //}

    public static void GetChannelValue(this in Rgba32 pixel, in ColorChannel channel, out byte value)
    {
        value = channel switch {
            ColorChannel.Red => pixel.R,
            ColorChannel.Green => pixel.G,
            ColorChannel.Blue => pixel.B,
            ColorChannel.Alpha => pixel.A,
            ColorChannel.Magnitude => GetLuma(in pixel),
            _ => 0,
        };
    }

    private static byte GetLuma(in Rgba32 pixel)
    {
        const float f = 1f / 255f;
        return (byte)(GetLuma(pixel.R * f, pixel.G * f, pixel.B * f) * 255f);
    }

    private static float GetLuma(in float red, in float green, in float blue)
    {
        return 0.299f * red + 0.587f * green + 0.114f * blue;
    }

    public static void SetChannelValue(this ref Rgb24 pixel, in ColorChannel channel, in byte value)
    {
        switch (channel) {
            case ColorChannel.Red:
                pixel.R = value;
                break;
            case ColorChannel.Green:
                pixel.G = value;
                break;
            case ColorChannel.Blue:
                pixel.B = value;
                break;
            case ColorChannel.Magnitude:
                pixel.R = pixel.G = pixel.B = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown color channel!");
        }
    }

    public static void SetChannelValue(this ref Rgba32 pixel, in ColorChannel channel, in byte value)
    {
        switch (channel) {
            case ColorChannel.Red:
                pixel.R = value;
                break;
            case ColorChannel.Green:
                pixel.G = value;
                break;
            case ColorChannel.Blue:
                pixel.B = value;
                break;
            case ColorChannel.Alpha:
                pixel.A = value;
                break;
            case ColorChannel.Magnitude:
                pixel.R = pixel.G = pixel.B = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown color channel!");
        }
    }

    //public static void GetChannelValueScaled(this in Rgb24 pixel, in ColorChannel channel, out float fValue)
    //{
    //    GetChannelValue(in pixel, in channel, out var value);
    //    fValue = value * ToScaled;
    //}

    public static void GetChannelValueScaled(this in Rgba32 pixel, in ColorChannel channel, out float fValue)
    {
        GetChannelValue(in pixel, in channel, out var value);
        fValue = (value + 0.5f) * ToScaled;
    }

    public static void GetChannelValueScaledF(this in L8 pixel, out float fValue)
    {
        fValue = pixel.PackedValue * ToScaled;
    }

    //public static void GetChannelValueScaledF(this in Rgb24 pixel, in ColorChannel channel, out float fValue)
    //{
    //    GetChannelValue(in pixel, in channel, out var value);
    //    fValue = value * ToScaled;
    //}

    public static void GetChannelValueScaledF(this in Rgba32 pixel, in ColorChannel channel, out float fValue)
    {
        GetChannelValue(in pixel, in channel, out var value);
        fValue = value * ToScaled;
    }

    public static void SetChannelValueScaled(this ref Rgba32 pixel, in ColorChannel channel, in float fValue)
    {
        MathEx.Saturate(in fValue, out var value);
        SetChannelValue(ref pixel, in channel, in value);
    }

    public static void SetChannelValueScaledF(this ref Rgb24 pixel, in ColorChannel channel, in float fValue)
    {
        MathEx.SaturateFloor(in fValue, out var value);
        SetChannelValue(ref pixel, in channel, in value);
    }

    public static void SetChannelValueScaledF(this ref Rgba32 pixel, in ColorChannel channel, in float fValue)
    {
        MathEx.SaturateFloor(in fValue, out var value);
        SetChannelValue(ref pixel, in channel, in value);
    }

    //public static void ToScaledVector3(this Rgb24 pixel, out Vector3 vector)
    //{
    //    vector.X = pixel.R * ToScaled;
    //    vector.Y = pixel.G * ToScaled;
    //    vector.Z = pixel.B * ToScaled;
    //}

    public static void ToNormalVector(this in Rgb24 pixel, out Vector3 vector)
    {
        vector.X = (pixel.R - 127.5f) * ToHalfScaled;
        vector.Y = (pixel.G - 127.5f) * ToHalfScaled;
        vector.Z = (pixel.B - 127.5f) * ToHalfScaled;
    }

    public static void FromNormalVector(this ref Rgb24 pixel, in Vector3 vector)
    {
        pixel.R = (byte)Math.Clamp((vector.X + 1f) * 127.5f, 0f, 255f);
        pixel.G = (byte)Math.Clamp((vector.Y + 1f) * 127.5f, 0f, 255f);
        pixel.B = (byte)Math.Clamp((vector.Z + 1f) * 127.5f, 0f, 255f);
    }
}