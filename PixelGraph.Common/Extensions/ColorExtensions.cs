using System;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Extensions
{
    internal static class ColorExtensions
    {
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

        public static void GetChannelValueScaled(this in Rgba32 pixel, in ColorChannel channel, out float value)
        {
            value = channel switch {
                ColorChannel.Red => pixel.R / 255f,
                ColorChannel.Green => pixel.G / 255f,
                ColorChannel.Blue => pixel.B / 255f,
                ColorChannel.Alpha => pixel.A / 255f,
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
    }
}
