using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.Extensions
{
    internal static class ColorExtensions
    {
        private const float ToScaled = 1f / 255f;

        public static void GetChannelValue(this in Rgb24 pixel, in ColorChannel channel, out byte value)
        {
            value = channel switch {
                ColorChannel.Red => pixel.R,
                ColorChannel.Green => pixel.G,
                ColorChannel.Blue => pixel.B,
                _ => 0,
            };
        }

        public static void GetChannelValue(this in Rgba32 pixel, in ColorChannel channel, out byte value)
        {
            value = channel switch {
                ColorChannel.Red => pixel.R,
                ColorChannel.Green => pixel.G,
                ColorChannel.Blue => pixel.B,
                ColorChannel.Alpha => pixel.A,
                _ => 0,
            };
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown color channel!");
            }
        }

        public static void GetChannelValueScaled(this in Rgb24 pixel, in ColorChannel channel, out float fValue)
        {
            GetChannelValue(in pixel, in channel, out var value);
            fValue = value * ToScaled;
        }

        public static void GetChannelValueScaled(this in Rgba32 pixel, in ColorChannel channel, out float fValue)
        {
            GetChannelValue(in pixel, in channel, out var value);
            fValue = (value + 0.5f) * ToScaled;
        }

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

        public static void SetChannelValueScaledF(this ref Rgba32 pixel, in ColorChannel channel, in float fValue)
        {
            MathEx.SaturateFloor(in fValue, out var value);
            SetChannelValue(ref pixel, in channel, in value);
        }
    }
}
