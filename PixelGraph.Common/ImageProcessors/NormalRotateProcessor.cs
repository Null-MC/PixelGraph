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
        private readonly bool hasRotation, hasCurveX, hasCurveY, hasNoise;


        public NormalRotateProcessor(Options options)
        {
            this.options = options;

            hasCurveX = MathF.Abs(options.CurveX) > float.Epsilon && options.RadiusX > float.Epsilon;
            hasCurveY = MathF.Abs(options.CurveY) > float.Epsilon && options.RadiusY > float.Epsilon;

            hasRotation = hasCurveX || hasCurveY;
            hasNoise = options.Noise > float.Epsilon;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixel = new Rgba32();
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(context.Bounds.Width, out noiseX);
                GenerateNoise(context.Bounds.Width, out noiseY);
            }

            float irx, iry, ox, oy;
            if (hasCurveX) {
                ox = 1f - options.RadiusX;
                irx = 1f / options.RadiusX;
            }
            else irx = ox = 0f;

            if (hasCurveY) {
                oy = 1f - options.RadiusY;
                iry = 1f / options.RadiusY;
            }
            else iry = oy = 0f;

            float fx, fy, rx, ry;
            float angleX, angleY;
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

                if (hasRotation || hasNoise) {
                    angleX = MathEx.AsinD(v.X);
                    angleY = MathEx.AsinD(v.Y);

                    if (hasCurveX) {
                        fx = (x - context.Bounds.X + 0.5f) / context.Bounds.Width * 2f - 1f;
                        rx = (MathF.Min(fx + ox, 0f) + MathF.Max(fx - ox, 0f)) * irx;
                        angleX += options.CurveX * rx * 0.5f;
                    }

                    if (hasCurveY) {
                        fy = (context.Y - context.Bounds.Y + 0.5f) / context.Bounds.Height * 2f - 1f;
                        ry = (MathF.Min(fy + oy, 0f) + MathF.Max(fy - oy, 0f)) * iry;
                        angleY += options.CurveY * ry * 0.5f;
                    }

                    if (hasNoise) {
                        angleX += (noiseX[x] / 127f - 1f) * options.Noise;
                        angleY += (noiseY[x] / 127f - 1f) * options.Noise;
                    }

                    AngleToVector(in angleX, in angleY, out v);
                }
                else if (options.RestoreNormalZ) {
                    var v2 = new Vector2(v.X, v.Y);
                    var d = Vector2.Dot(v2, v2);
                    v.Z = MathF.Sqrt(1f - d);
                }

                pixel.SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Blue, v.Z * 0.5f + 0.5f);

                row[x].FromRgba32(pixel);
            }
        }

        protected override void ProcessRow(in PixelRowContext context, Span<Rgba32> row)
        {
            byte[] noiseX = null, noiseY = null;

            if (hasNoise) {
                GenerateNoise(context.Bounds.Width, out noiseX);
                GenerateNoise(context.Bounds.Width, out noiseY);
            }

            float irx, iry, ox, oy;
            if (hasCurveX) {
                ox = 1f - options.RadiusX;
                irx = 1f / options.RadiusX;
            }
            else irx = ox = 0f;

            if (hasCurveY) {
                oy = 1f - options.RadiusY;
                iry = 1f / options.RadiusY;
            }
            else iry = oy = 0f;
            
            float fx, fy, rx, ry;
            float angleX, angleY;
            var v = new Vector3();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].GetChannelValue(ColorChannel.Red, out var normalX);
                row[x].GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                if (!options.RestoreNormalZ) {
                    row[x].GetChannelValueScaled(ColorChannel.Blue, out var normalZ);
                    v.Z = Math.Clamp(normalZ / 127f - 1f, -1f, 1f);
                    MathEx.Normalize(ref v);
                }

                if (hasRotation || hasNoise) {
                    angleX = MathEx.AsinD(v.X);
                    angleY = MathEx.AsinD(v.Y);

                    if (hasCurveX) {
                        fx = (x - context.Bounds.X + 0.5f) / context.Bounds.Width * 2f - 1f;
                        rx = (MathF.Min(fx + ox, 0f) + MathF.Max(fx - ox, 0f)) * irx;
                        angleX += options.CurveX * (rx - 0.5f);
                    }

                    if (hasCurveY) {
                        fy = (context.Y - context.Bounds.Y + 0.5f) / context.Bounds.Height * 2f - 1f;
                        ry = (MathF.Min(fy + oy, 0f) + MathF.Max(fy - oy, 0f)) * iry;
                        angleY += options.CurveY * (ry - 0.5f);
                    }

                    if (hasNoise) {
                        angleX += (noiseX[x] / 127f - 1f) * options.Noise;
                        angleY += (noiseY[x] / 127f - 1f) * options.Noise;
                    }

                    AngleToVector(in angleX, in angleY, out v);
                }
                else if (options.RestoreNormalZ) {
                    var v2 = new Vector2(v.X, v.Y);
                    var d = Vector2.Dot(v2, v2);
                    v.Z = MathF.Sqrt(1f - d);
                }

                row[x].SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                row[x].SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                row[x].SetChannelValueScaledF(ColorChannel.Blue, v.Z * 0.5f + 0.5f);
            }
        }

        private static void AngleToVector(in float angleX, in float angleY, out Vector3 vector)
        {
            //MathEx.Clamp(ref angleX, -90f, 90f);
            //MathEx.Clamp(ref angleY, -90f, 90f);

            var sinX = MathEx.SinD(angleX);
            var cosX = MathEx.CosD(angleX);
            var sinY = MathEx.SinD(angleY);
            var cosY = MathEx.CosD(angleY);

            vector.X = sinX * cosY;
            vector.Y = sinY * cosX;
            vector.Z = cosX * cosY;
            MathEx.Normalize(ref vector);
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
            public float CurveX = 0f;
            public float CurveY = 0f;
            public float RadiusX = 1f;
            public float RadiusY = 1f;
            public float Noise = 0f;
        }
    }
}
