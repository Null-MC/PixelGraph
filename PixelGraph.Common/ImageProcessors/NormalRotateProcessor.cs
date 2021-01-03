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

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixel);
                pixel.GetChannelValue(options.NormalX, out var normalX);
                pixel.GetChannelValue(options.NormalY, out var normalY);

                var cx = Math.Clamp(normalX / 127f - 1f, -1f, 1f);
                var cy = Math.Clamp(normalY / 127f - 1f, -1f, 1f);

                var angleX = MathF.Asin(cx) / MathEx.Deg2Rad;
                var angleY = MathF.Asin(cy) / MathEx.Deg2Rad;

                if (hasRotation) {
                    var fx = (x + 0.5f) / context.Bounds.Width;
                    var fy = (context.Y + 0.5f) / context.Bounds.Height;
                    angleX += options.CurveX * (fx - 0.5f);
                    angleY += options.CurveY * (fy - 0.5f);
                }

                if (hasNoise) {
                    //context.GetNoise(in x, out var noiseX, out var noiseY);
                    angleX += (noiseX[x] / 127f - 1f) * options.Noise;
                    angleY += (noiseY[x] / 127f - 1f) * options.Noise;
                }

                MathEx.Clamp(ref angleX, -90f, 90f);
                MathEx.Clamp(ref angleY, -90f, 90f);

                var sinX = MathF.Sin(angleX * MathEx.Deg2Rad);
                var cosX = MathF.Cos(angleX * MathEx.Deg2Rad);
                var sinY = MathF.Sin(angleY * MathEx.Deg2Rad);
                var cosY = MathF.Cos(angleY * MathEx.Deg2Rad);

                Vector3 v;
                v.X = sinX * cosY;
                v.Y = sinY * cosX;
                v.Z = cosX * cosY;
                MathEx.Normalize(ref v);

                if (options.NormalX != ColorChannel.None)
                    pixel.SetChannelValueScaledF(options.NormalX, v.X * 0.5f + 0.5f);

                if (options.NormalY != ColorChannel.None)
                    pixel.SetChannelValueScaledF(options.NormalY, v.Y * 0.5f + 0.5f);

                if (options.NormalZ != ColorChannel.None)
                    pixel.SetChannelValueScaledF(options.NormalZ, v.Z);

                row[x].FromRgba32(pixel);
            }
        }

        private static void GenerateNoise(in int size, out byte[] buffer)
        {
            var random = new Random();
            buffer = new byte[size];
            random.NextBytes(buffer);
        }

        public class Options //<TPixel>
        {
            public ColorChannel NormalX = ColorChannel.None;
            public ColorChannel NormalY = ColorChannel.None;
            public ColorChannel NormalZ = ColorChannel.None;
            //public ResourcePackChannelProperties MagnitudeChannel;
            //public Image<TP> MagnitudeImage;
            public float CurveX = 0f;
            public float CurveY = 0f;
            public float Noise = 0f;
        }
    }
}
