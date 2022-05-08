using PixelGraph.Common.Extensions;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal enum NormalEncodings
    {
		Standard,
        CompressedProject,
        CompressedAngle,
        UniformAngle,
        Octahedron,
    }

    internal static class NormalEncoding
    {
        private static NormalEncodings defaultEncoding => NormalEncodings.Standard;

        public static void EncodeNormal(in Vector3 normal, out Vector3 result) => 
            EncodeNormal(in normal, defaultEncoding, out result);

        public static void DecodeNormal(in Vector3 normal, bool restoreZ, out Vector3 result) => 
            DecodeNormal(in normal, defaultEncoding, restoreZ, out result);

        public static void EncodeNormal(in Vector3 normal, in NormalEncodings encoding, out Vector3 result)
        {
            switch (encoding) {
                case NormalEncodings.CompressedProject:
                    Vector2 normalizedNormal1;
                    normalizedNormal1.X = normal.X;
                    normalizedNormal1.Y = normal.Y;

                    if (normalizedNormal1.LengthSquared() >= float.Epsilon) {
                        MathEx.Normalize(ref normalizedNormal1);

                        var max1 = MathF.Max(MathF.Abs(normalizedNormal1.X), MathF.Abs(normalizedNormal1.Y));
                        result.X = normal.X / max1;
                        result.Y = normal.Y / max1;
                    }
                    else {
                        result.X = result.Y = 0f;
                    }

                    result.Z = 0f;
                    break;

                case NormalEncodings.CompressedAngle or NormalEncodings.UniformAngle:
                    Vector2 normalizedNormal2;
                    normalizedNormal2.X = normal.X;
                    normalizedNormal2.Y = normal.Y;

                    if (normalizedNormal2.LengthSquared() >= float.Epsilon) {
                        MathEx.Normalize(ref normalizedNormal2);

                        var radius = MathF.Acos(normal.Z) / (MathF.PI * 0.5f);
                        var max2 = MathF.Max(MathF.Abs(normalizedNormal2.X), MathF.Abs(normalizedNormal2.Y));
                        result.X = normalizedNormal2.X * radius / max2;
                        result.Y = normalizedNormal2.Y * radius / max2;
            
                        if (encoding == NormalEncodings.UniformAngle) {
                            Vector2 absNormal2;
                            absNormal2.X = MathF.Abs(result.X);
                            absNormal2.Y = MathF.Abs(result.Y);

                            if (absNormal2.X > absNormal2.Y) {
                                result.Y = MathF.Atan(result.Y / absNormal2.X) * (4f / MathF.PI) * absNormal2.X;
                            }
                            else {
                                result.X = MathF.Atan(result.X / absNormal2.Y) * (4f / MathF.PI) * absNormal2.Y;
                            }
                        }
                    }
                    else {
                        result.X = result.Y = 0f;
                    }

                    result.Z = 0f;
                    break;

                case NormalEncodings.Octahedron:
                    throw new NotImplementedException();
                    //var l = MathF.Abs(normal.X) + MathF.Abs(normal.Y) + MathF.Abs(normal.Z);
                    //result = Vector3.Divide(normal, l) * mat2(1.0, 1.0, -1.0, 1.0);
                    //break;

                default:
                    MathEx.Normalize(in normal, out result);
                    //result = Vector3.Multiply(result, 0.5f);
                    //result.X += 0.5f;
                    //result.Y += 0.5f;
                    //result.Z += 0.5f;
                    break;
            }
        }

        public static void DecodeNormal(in Vector3 normal, in NormalEncodings encoding, bool restoreZ, out Vector3 result)
        {
            //result = Vector3.Multiply(normal2, 2f);
            //result.X -= 1f;
            //result.Y -= 1f;
            //result.Z -= 1f;

            switch (encoding) {
                case NormalEncodings.CompressedProject:
                    Vector2 normalizedNormal1;
                    normalizedNormal1.X = normal.X;
                    normalizedNormal1.Y = normal.Y;
                    MathEx.Normalize(ref normalizedNormal1);

                    var max1 = MathF.Max(MathF.Abs(normalizedNormal1.X), MathF.Abs(normalizedNormal1.Y));
                    result.X = normal.X * max1;
                    result.Y = normal.Y * max1;

                    var dot1 = result.X*result.X + result.Y*result.Y;
                    result.Z = MathF.Sqrt(MathF.Max(1f - dot1, 0f));
                    break;

                case NormalEncodings.CompressedAngle or NormalEncodings.UniformAngle:
                    result = normal;

                    if (encoding == NormalEncodings.UniformAngle) {
                        Vector2 absNormal3;
                        absNormal3.X = MathF.Abs(normal.X);
                        absNormal3.Y = MathF.Abs(normal.Y);

                        if (absNormal3.X > absNormal3.Y) {
                            result.Y = MathF.Tan(normal.Y * (MathF.PI / 4f) / absNormal3.X) * absNormal3.X;
                        }
                        else {
                            result.X = MathF.Tan(normal.X * (MathF.PI / 4f) / absNormal3.Y) * absNormal3.Y;
                        }
                    }

                    Vector2 normalizedNormal2;
                    normalizedNormal2.X = result.X;
                    normalizedNormal2.Y = result.Y;
                    MathEx.Normalize(ref normalizedNormal2);

                    var max2 = MathF.Max(MathF.Abs(normalizedNormal2.X), MathF.Abs(normalizedNormal2.Y));
                    result.X *= max2;
                    result.Y *= max2;
		        
                    var dot2 = result.X*result.X + result.Y*result.Y;
                    var angle = MathF.Sqrt(dot2) * (MathF.PI * 0.5f);
                    var sinAngle = MathF.Sin(angle);
                    result.X = sinAngle * normalizedNormal2.X;
                    result.Y = sinAngle * normalizedNormal2.Y;
                    result.Z = MathF.Cos(angle);
                    break;

                case NormalEncodings.Octahedron:
                    throw new NotImplementedException();
                    //result.xy = normal3.xy * mat2(0.5, -0.5, 0.5, 0.5);

                    //var m = new Matrix3x2(0.5f, 0f, 0f, -0.5f, 0.5f, 0.5f);
                    //var xy = new Vector2(normal3.X, normal3.Y);
                    //Matrix.Multiply() * m;

                    //result.Z = 1f - MathF.Abs(normal3.X) - MathF.Abs(normal3.Y);
                    //MathEx.Normalize(ref result);

                default:
                    result = normal;

                    if (restoreZ) {
                        var len = MathF.Min(normal.X*normal.X + normal.Y*normal.Y, 1f);
                        result.Z = MathF.Sqrt(1f - len);
                    }

                    MathEx.Normalize(ref result);
                    break;
            }
        }
    }
}
