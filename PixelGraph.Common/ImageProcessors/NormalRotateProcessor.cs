using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalRotateProcessor : PixelRowProcessor
    {
        private readonly Options options;
        private readonly bool hasRotation, hasNoise;
        private readonly float offsetTop, offsetBottom, offsetLeft, offsetRight;
        private readonly float invRadiusTop, invRadiusBottom, invRadiusLeft, invRadiusRight;


        public NormalRotateProcessor(Options options)
        {
            this.options = options;

            var hasCurveTop = MathF.Abs(options.CurveTop) > float.Epsilon && options.RadiusTop > float.Epsilon;
            var hasCurveBottom = MathF.Abs(options.CurveBottom) > float.Epsilon && options.RadiusBottom > float.Epsilon;
            var hasCurveLeft = MathF.Abs(options.CurveLeft) > float.Epsilon && options.RadiusLeft > float.Epsilon;
            var hasCurveRight = MathF.Abs(options.CurveRight) > float.Epsilon && options.RadiusRight > float.Epsilon;

            hasRotation = hasCurveTop || hasCurveBottom || hasCurveLeft || hasCurveRight;
            hasNoise = options.Noise > float.Epsilon;

            if (hasCurveTop) {
                offsetTop = 1f - options.RadiusTop;
                invRadiusTop = 1f / options.RadiusTop;
            }
            else invRadiusTop = offsetTop = 0f;

            if (hasCurveBottom) {
                offsetBottom = 1f - options.RadiusBottom;
                invRadiusBottom = 1f / options.RadiusBottom;
            }
            else invRadiusBottom = offsetBottom = 0f;

            if (hasCurveLeft) {
                offsetLeft = 1f - options.RadiusLeft;
                invRadiusLeft = 1f / options.RadiusLeft;
            }
            else invRadiusLeft = offsetLeft = 0f;

            if (hasCurveRight) {
                offsetRight = 1f - options.RadiusRight;
                invRadiusRight = 1f / options.RadiusRight;
            }
            else invRadiusRight = offsetRight = 0f;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixel = new Rgba32();
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(context.Bounds.Width, out noiseX);
                GenerateNoise(context.Bounds.Width, out noiseY);
            }

            var v = new Vector3();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                if (x < 0 || x >= row.Length) continue;

                row[x].ToRgba32(ref pixel);
                pixel.GetChannelValue(ColorChannel.Red, out var normalX);
                pixel.GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                if (!options.RestoreNormalZ) {
                    pixel.GetChannelValue(ColorChannel.Blue, out var normalZ);
                    v.Z = Math.Clamp(normalZ / 127f - 1f, -1f, 1f);
                    MathEx.Normalize(ref v);
                }

                ProcessPixel(in context, ref v, in noiseX, in noiseY, in x);

                pixel.SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Blue, v.Z * 0.5f + 0.5f);

                row[x].FromRgba32(pixel);
            }
        }

        private void ProcessPixel(in PixelRowContext context, ref Vector3 v, in byte[] noiseX, in byte[] noiseY, in int x)
        {
            if (hasRotation || hasNoise) {
                var fx = (x - context.Bounds.X + 0.5f) / context.Bounds.Width * 2f - 1f;
                var fy = (context.Y - context.Bounds.Y + 0.5f) / context.Bounds.Height * 2f - 1f;

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
                    var angle_x = MathF.Sign(-f_left) * options.CurveLeft + MathF.Sign(f_right) * options.CurveRight;
                    var angle_y = MathF.Sign(-f_top) * options.CurveTop + MathF.Sign(f_bottom) * options.CurveBottom;
                    MathEx.Lerp(angle_x, angle_y, mix, out var angle);
                    
                    q = Quaternion.CreateFromAxisAngle(rotationAxis, baseAngle * (angle / 45f));
                }

                if (hasNoise) {
                    var z = x - context.Bounds.Left;
                    q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (noiseX[z] / 127.5f - 1f) * options.Noise * MathEx.Deg2RadF);
                    q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, (noiseY[z] / 127.5f - 1f) * -options.Noise * MathEx.Deg2RadF);
                }

                v = Vector3.Transform(v, q);
            }
            else if (options.RestoreNormalZ) {
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

        public class Options
        {
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
        }
    }
}
