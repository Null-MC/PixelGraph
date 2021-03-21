using PixelGraph.Common.Textures;
using System;
using System.Numerics;

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

        public static void ToEuler(this in Vector3 vector, out Vector2 angle)
        {
            angle.X = MathF.Asin(vector.X) / MathEx.Deg2Rad;
            angle.Y = MathF.Asin(vector.Y) / MathEx.Deg2Rad;
        }

        public static void FromEuler(this ref Vector3 vector, in Vector2 angle)
        {
            var sinX = MathF.Sin(angle.X * MathEx.Deg2Rad);
            var cosX = MathF.Cos(angle.X * MathEx.Deg2Rad);
            var sinY = MathF.Sin(angle.Y * MathEx.Deg2Rad);
            var cosY = MathF.Cos(angle.Y * MathEx.Deg2Rad);

            vector.X = sinX * cosY;
            vector.Y = sinY * cosX;
            vector.Z = cosX * cosY;

            if (vector.LengthSquared() >= float.Epsilon) MathEx.Normalize(ref vector);
            else vector = Vector3.UnitZ;
        }

        public static void Add(this ref Vector3 vector, in Vector3 addVector)
        {
            vector.X += addVector.X;
            vector.Y += addVector.Y;
            vector.Z += addVector.Z;
        }
    }
}
