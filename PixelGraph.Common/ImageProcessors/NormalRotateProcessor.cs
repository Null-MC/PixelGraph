using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalRotateProcessor : PixelRowProcessor
    {
        private bool hasRotation, hasNoise;
        private float offsetTop, offsetBottom, offsetLeft, offsetRight;
        private float invRadiusTop, invRadiusBottom, invRadiusLeft, invRadiusRight;

        public Rectangle Bounds;
        public bool RestoreNormalZ;
        public float CurveTop = 0f;
        public float CurveBottom = 0f;
        public float CurveLeft = 0f;
        public float CurveRight = 0f;
        public float RadiusTop = 1f;
        public float RadiusBottom = 1f;
        public float RadiusLeft = 1f;
        public float RadiusRight = 1f;
        public float Noise = 0f;


        public void Apply(Image image)
        {
            var hasCurveTop = MathF.Abs(CurveTop) > float.Epsilon && RadiusTop > float.Epsilon;
            var hasCurveBottom = MathF.Abs(CurveBottom) > float.Epsilon && RadiusBottom > float.Epsilon;
            var hasCurveLeft = MathF.Abs(CurveLeft) > float.Epsilon && RadiusLeft > float.Epsilon;
            var hasCurveRight = MathF.Abs(CurveRight) > float.Epsilon && RadiusRight > float.Epsilon;

            hasRotation = hasCurveTop || hasCurveBottom || hasCurveLeft || hasCurveRight;
            hasNoise = Noise > float.Epsilon;

            if (hasCurveTop) {
                offsetTop = 1f - RadiusTop;
                invRadiusTop = 1f / RadiusTop;
            }
            else invRadiusTop = offsetTop = 0f;

            if (hasCurveBottom) {
                offsetBottom = 1f - RadiusBottom;
                invRadiusBottom = 1f / RadiusBottom;
            }
            else invRadiusBottom = offsetBottom = 0f;

            if (hasCurveLeft) {
                offsetLeft = 1f - RadiusLeft;
                invRadiusLeft = 1f / RadiusLeft;
            }
            else invRadiusLeft = offsetLeft = 0f;

            if (hasCurveRight) {
                offsetRight = 1f - RadiusRight;
                invRadiusRight = 1f / RadiusRight;
            }
            else invRadiusRight = offsetRight = 0f;

            image.Mutate(c => {
                c.ProcessPixelRowsAsVector4(ProcessRow);
            });
        }

        private void ProcessRow(Span<Vector4> row, Point pos)
        {
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(Bounds.Width, out noiseX);
                GenerateNoise(Bounds.Width, out noiseY);
            }

            var v = new Vector3();
            for (var x = Bounds.Left; x < Bounds.Right; x++) {
                if (x < 0 || x >= row.Length) continue;

                row[x].GetChannelValue(ColorChannel.Red, out var normalX);
                row[x].GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX * 2f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY * 2f - 1f, -1f, 1f);

                if (!RestoreNormalZ) {
                    row[x].GetChannelValue(ColorChannel.Blue, out var normalZ);
                    v.Z = Math.Clamp(normalZ * 2f - 1f, -1f, 1f);
                    MathEx.Normalize(ref v);
                }

                ProcessPixel(in x, pos.Y, ref v, in noiseX, in noiseY);

                row[x].SetChannelValue(ColorChannel.Red, v.X * 0.5f + 0.5f);
                row[x].SetChannelValue(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                row[x].SetChannelValue(ColorChannel.Blue, v.Z * 0.5f + 0.5f);
            }
        }

        private void ProcessPixel(in int x, in int y, ref Vector3 v, in byte[] noiseX, in byte[] noiseY)
        {
            if (hasRotation || hasNoise) {
                var fx = (x - Bounds.X + 0.5f) / Bounds.Width * 2f - 1f;
                var fy = (y - Bounds.Y + 0.5f) / Bounds.Height * 2f - 1f;

                var f_top = MathF.Min(fy + offsetTop, 0f) * invRadiusTop;
                var f_bottom = MathF.Max(fy - offsetBottom, 0f) * invRadiusBottom;
                var f_left = MathF.Min(fx + offsetLeft, 0f) * invRadiusLeft;
                var f_right = MathF.Max(fx - offsetRight, 0f) * invRadiusRight;

                var baseRotation = Vector3.One;
                baseRotation.X = f_left + f_right;
                baseRotation.Y = f_top + f_bottom;

                var q = Quaternion.Identity;
                if (!baseRotation.X.NearEqual(0f) || !baseRotation.Y.NearEqual(0f)) {
                    var rotationAxis = new Vector3 {
                        X = -baseRotation.Y,
                        Y = baseRotation.X,
                        Z = 0f,
                    };

                    MathEx.Normalize(ref rotationAxis);
                    MathEx.Normalize(ref baseRotation);
                    var baseAngle = MathF.Acos(baseRotation.Z);

                    var mix = MathF.Abs(MathF.Asin(rotationAxis.X) / (MathF.PI / 2f));
                    var angle_x = MathF.Sign(-f_left) * CurveLeft + MathF.Sign(f_right) * CurveRight;
                    var angle_y = MathF.Sign(-f_top) * CurveTop + MathF.Sign(f_bottom) * CurveBottom;
                    MathEx.Lerp(angle_x, angle_y, mix, out var angle);
                    
                    q = Quaternion.CreateFromAxisAngle(rotationAxis, baseAngle * (angle / 45f));
                }

                if (hasNoise) {
                    var z = x - Bounds.Left;
                    q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (noiseX[z] / 127.5f - 1f) * Noise * MathEx.Deg2RadF);
                    q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, (noiseY[z] / 127.5f - 1f) * -Noise * MathEx.Deg2RadF);
                }

                v = Vector3.Transform(v, q);
            }
            else if (RestoreNormalZ) {
                var v2 = new Vector2(v.X, v.Y);
                var d = Vector2.Dot(v2, v2);
                MathEx.Clamp(ref d, 0f, 1f);
                v.Z = MathF.Sqrt(1f - d);
            }

            MathEx.Normalize(ref v);
        }

        private static void GenerateNoise(in int size, out byte[] buffer)
        {
            var random = new Random();
            buffer = new byte[size];
            random.NextBytes(buffer);
        }
    }
}
