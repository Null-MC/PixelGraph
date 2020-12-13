using System;
using System.Numerics;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Extensions
{
    internal static class VectorExtensions
    {
        public static void GetChannelValue(this in Vector4 vector, in ColorChannel channel, out float value)
        {
            value = channel switch {
                ColorChannel.Red => vector.X,
                ColorChannel.Green => vector.Y,
                ColorChannel.Blue => vector.Z,
                ColorChannel.Alpha => vector.W,
                _ => 0f,
            };
        }

        public static void SetChannelValue(this ref Vector4 vector, in ColorChannel channel, in float value)
        {
            switch (channel) {
                case ColorChannel.Red:
                    vector.X = value;
                    break;
                case ColorChannel.Green:
                    vector.Y = value;
                    break;
                case ColorChannel.Blue:
                    vector.Z = value;
                    break;
                case ColorChannel.Alpha:
                    vector.W = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown vector channel!");
            }
        }
    }
}
