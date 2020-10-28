using System;
using System.Numerics;

namespace McPbrPipeline.Internal.Extensions
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

        public static void Saturate(float value, out byte result)
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
    }
}
