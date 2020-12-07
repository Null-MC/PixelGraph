using System;
using System.Numerics;

namespace PixelGraph.Common.Extensions
{
    internal static class MathEx
    {
        public const float Deg2Rad = (float)(Math.PI / 180);


        public static void Normalize(ref Vector3 value)
        {
            float length;
            if (Vector.IsHardwareAccelerated) {
                length = value.Length();
            }
            else {
                var ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
                length = MathF.Sqrt(ls);
            }

            value.X /= length;
            value.Y /= length;
            value.Z /= length;
        }

        public static byte Clamp(float value)
        {
            return (byte)Math.Clamp(value + 0.5f, 0f, 255f);
        }

        public static void Clamp(ref float value, in float min, in float max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
        }

        public static void Saturate(in float value, out byte result)
        {
            result = (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        }

        public static byte Saturate(float value)
        {
            return (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        }

        public static byte Saturate(double value)
        {
            return (byte)Math.Clamp(value * 255d + 0.5d, 0, 255);
        }

        public static void Lerp(in float min, in float max, in float mix, out float result)
        {
            result = min * (1f - mix) + max * mix;
        }

        public static void Cycle(ref byte value, in int offset)
        {
            if (offset == 0) return;
            var x = value + offset;
            while (x > 255) x -= 256;
            while (x < 0) x += 256;
            value = (byte)x;
        }
    }
}
