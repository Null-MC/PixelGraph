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


        public NormalRotateProcessor(Options options)
        {
            this.options = options;

            hasRotation = options.CurveX > float.Epsilon || options.CurveY > float.Epsilon;
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

            float angleX, angleY;
            var v = new Vector3();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixel);
                pixel.GetChannelValue(ColorChannel.Red, out var normalX);
                pixel.GetChannelValue(ColorChannel.Green, out var normalY);

                v.X = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                v.Y = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                if (options.HasNormalZ) {
                    pixel.GetChannelValueScaledF(ColorChannel.Blue, out v.Z);
                    MathEx.Normalize(ref v);
                }

                angleX = MathF.Asin(v.X) / MathEx.Deg2Rad;
                angleY = MathF.Asin(v.Y) / MathEx.Deg2Rad;

                if (hasRotation) {
                    var fx = (x + 0.5f) / context.Bounds.Width;
                    var fy = (context.Y + 0.5f) / context.Bounds.Height;
                    angleX += options.CurveX * (fx - 0.5f);
                    angleY += options.CurveY * (fy - 0.5f);
                }

                if (hasNoise) {
                    angleX += (noiseX[x] / 127f - 1f) * options.Noise;
                    angleY += (noiseY[x] / 127f - 1f) * options.Noise;
                }

                MathEx.Clamp(ref angleX, -90f, 90f);
                MathEx.Clamp(ref angleY, -90f, 90f);

                var sinX = MathF.Sin(angleX * MathEx.Deg2Rad);
                var cosX = MathF.Cos(angleX * MathEx.Deg2Rad);
                var sinY = MathF.Sin(angleY * MathEx.Deg2Rad);
                var cosY = MathF.Cos(angleY * MathEx.Deg2Rad);

                v.X = sinX * cosY;
                v.Y = sinY * cosX;
                v.Z = cosX * cosY;
                MathEx.Normalize(ref v);

                pixel.SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                pixel.SetChannelValueScaledF(ColorChannel.Blue, v.Z);

                row[x].FromRgba32(pixel);
            }
        }

        private static void GenerateNoise(in int size, out byte[] buffer)
        {
            var random = new Random();
            buffer = new byte[size];
            random.NextBytes(buffer);
        }

        public class Options
        {
            public bool HasNormalZ;
            public float CurveX = 0f;
            public float CurveY = 0f;
            public float Noise = 0f;
        }
    }
}
