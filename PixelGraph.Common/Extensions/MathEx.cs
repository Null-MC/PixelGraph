using System;
using System.Numerics;

namespace PixelGraph.Common.Extensions
{
    public static class MathEx
    {
        public const float Deg2RadF = (float)(Math.PI / 180d);
        public const float Rad2DegF = (float)(180d / Math.PI);
        public const double Deg2Rad = Math.PI / 180d;
        public const double Rad2Deg = 180d / Math.PI;


        public static float CosD(float d) => MathF.Cos(d * Deg2RadF);
        public static double CosD(double d) => Math.Cos(d * Deg2Rad);

        public static float SinD(float d) => MathF.Sin(d * Deg2RadF);
        public static double SinD(double d) => Math.Sin(d * Deg2Rad);

        public static float AsinD(float d) => MathF.Asin(d) * Rad2DegF;
        public static double AsinD(double d) => Math.Asin(d) * Rad2Deg;

        public static bool NearEqual(this in float valueA, in float valueB)
        {
            return MathF.Abs(valueA - valueB) < float.Epsilon;
        }

        //public static bool Equal(this double valueA, double valueB)
        //{
        //    return Math.Abs(valueA - valueB) < double.Epsilon;
        //}

        public static void Normalize(ref Vector3 value)
        {
            float lengthF;
            if (Vector.IsHardwareAccelerated) {
                lengthF = 1f / value.Length();
            }
            else {
                var ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
                lengthF = 1f / MathF.Sqrt(ls);
            }

            if (lengthF.NearEqual(1f)) return;

            value.X *= lengthF;
            value.Y *= lengthF;
            value.Z *= lengthF;
        }

        public static void Normalize(ref Vector4 value)
        {
            var length2 = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            var lengthF = 1f / MathF.Sqrt(length2);
            if (lengthF.NearEqual(1f)) return;

            value.X *= lengthF;
            value.Y *= lengthF;
            value.Z *= lengthF;
        }

        //public static void Invert(ref byte value)
        //{
        //    value = (byte) (255 - value);
        //}

        //public static void Invert(ref float value)
        //{
        //    value = 1f - value;
        //}

        public static void Invert(ref float value, in float minValue, in float maxValue)
        {
            value = maxValue - (value - minValue);
        }

        //public static void Invert(ref double value, in double minValue, in double maxValue)
        //{
        //    value = maxValue - (value - minValue);
        //}

        //public static byte Clamp(int value)
        //{
        //    return (byte)Math.Clamp(value, 0, 255);
        //}

        //public static byte Clamp(int value, byte min, byte max)
        //{
        //    return (byte)Math.Clamp(value, min, max);
        //}

        //public static byte Clamp(float value, byte min, byte max)
        //{
        //    return (byte)Math.Clamp((int)(value + 0.5f), min, max);
        //}

        public static byte ClampRound(in float value, in byte min, in byte max)
        {
            return (byte)Math.Clamp((int)(value + 0.5f), min, max);
        }

        //public static byte Clamp(float value)
        //{
        //    return (byte)Math.Clamp(value + 0.5f, 0f, 255f);
        //}

        public static void Clamp(ref float value, in float min, in float max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
        }

        //public static void Clamp(ref double value, in double min, in double max)
        //{
        //    if (value < min) value = min;
        //    else if (value > max) value = max;
        //}

        public static void Saturate(in float value, out byte result)
        {
            result = (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        }

        //public static void Saturate(in double value, out byte result)
        //{
        //    result = (byte)Math.Clamp(value * 255d + 0.5d, 0d, 255d);
        //}

        public static void SaturateFloor(in float value, out byte result)
        {
            result = (byte)Math.Clamp(value * 255f, 0f, 255f);
        }

        //public static byte Saturate(float value)
        //{
        //    return (byte)Math.Clamp(value * 255f + 0.5f, 0f, 255f);
        //}

        //public static byte Saturate(double value)
        //{
        //    return (byte)Math.Clamp(value * 255d + 0.5d, 0, 255);
        //}

        #region Lerp

        public static void Lerp(in float min, in float max, in float mix, out float result)
        {
            result = min * (1f - mix) + max * mix;
        }

        public static void Lerp(in Vector2 min, in Vector2 max, in float mix, out Vector2 result)
        {
            Lerp(in min.X, in max.X, in mix, out result.X);
            Lerp(in min.Y, in max.Y, in mix, out result.Y);
        }

        //public static void Lerp(in Vector3 min, in Vector3 max, in float mix, out Vector3 result)
        //{
        //    Lerp(in min.X, in max.X, in mix, out result.X);
        //    Lerp(in min.Y, in max.Y, in mix, out result.Y);
        //    Lerp(in min.Z, in max.Z, in mix, out result.Z);
        //}

        public static void Lerp(in Vector4 min, in Vector4 max, in float mix, out Vector4 result)
        {
            Lerp(in min.X, in max.X, in mix, out result.X);
            Lerp(in min.Y, in max.Y, in mix, out result.Y);
            Lerp(in min.Z, in max.Z, in mix, out result.Z);
            Lerp(in min.W, in max.W, in mix, out result.W);
        }

        #endregion

        #region SmoothLerp

        //public static void SmoothLerp(in float min, in float max, in float mix, out float result)
        //{
        //    var f = (mix - min) / (max - min);
        //    Clamp(ref f, 0f, 1f);

        //    result = f * f * f * (f * (f * 6 - 15) + 10);
        //}

        //public static void SmoothLerp(in Vector2 min, in Vector2 max, in float mix, out Vector2 result)
        //{
        //    SmoothLerp(in min.X, in max.X, in mix, out result.X);
        //    SmoothLerp(in min.Y, in max.Y, in mix, out result.Y);
        //}

        //public static void SmoothLerp(in Vector3 min, in Vector3 max, in float mix, out Vector3 result)
        //{
        //    SmoothLerp(in min.X, in max.X, in mix, out result.X);
        //    SmoothLerp(in min.Y, in max.Y, in mix, out result.Y);
        //    SmoothLerp(in min.Z, in max.Z, in mix, out result.Z);
        //}

        #endregion

        public static void Wrap(ref int value, in int min, in int max)
        {
            while (value < min) value += max - min;
            while (value > max) value -= max - min;
        }

        //public static void Cycle(ref byte value, in int offset)
        //{
        //    var x = value + offset;
        //    while (x < 0) x += 256;
        //    while (x >= 256) x -= 256;
        //    value = (byte)x;
        //}

        public static void Cycle(ref byte value, in int offset, in byte min, in byte max)
        {
            var x = value + offset;
            while (x < min) x += max - min + 1;
            while (x > max) x -= max - min + 1;
            value = (byte)x;
        }

        public static void Cycle(ref float value, in int offset, in byte min, in byte max)
        {
            //const float r = (float)(256d / 255d);

            value += offset / 255f;
            while (value < min) value += (max - min) / 255f;
            while (value > max) value -= (max - min) / 255f;
        }

        //public static void PerceptualToLinear(ref double value)
        //{
        //    value = 1d - value;
        //    value *= value;
        //    value = 1d - value;
        //}

        //public static void LinearToPerceptual(ref double value)
        //{
        //    value = 1d - Math.Sqrt(1d - value);
        //}
    }
}
