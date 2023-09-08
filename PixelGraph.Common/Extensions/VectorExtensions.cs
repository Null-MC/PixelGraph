using PixelGraph.Common.Textures;
using System;
using System.Numerics;

namespace PixelGraph.Common.Extensions;

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

    public static void GetChannelByteValue(this in Vector4 vector, in ColorChannel channel, out byte value)
    {
        GetChannelValue(in vector, in channel, out var fValue);
        MathEx.Saturate(in fValue, out value);
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
        angle.X = MathEx.AsinD(vector.X);
        angle.Y = MathEx.AsinD(vector.Y);
    }

    public static void FromEuler(this ref Vector3 vector, in Vector2 angle)
    {
        var sinX = MathEx.SinD(angle.X);
        var cosX = MathEx.CosD(angle.X);
        var sinY = MathEx.SinD(angle.Y);
        var cosY = MathEx.CosD(angle.Y);

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

    public static void Add(this ref Vector3 vector, in float addValue)
    {
        vector.X += addValue;
        vector.Y += addValue;
        vector.Z += addValue;
    }

    //public static float Length3(this ref Vector4 vector)
    //{
    //    return MathF.Sqrt(vector.X*vector.X + vector.Y*vector.Y + vector.Z*vector.Z);
    //}

    //public static Vector3 operator +(Vector3 a, Vector3 b)
    //{
    //    return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    //}
}