using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalRotateProcessor : PixelProcessor
    {
        private readonly Options options;
        private readonly bool hasRotation, hasNoise;


        public NormalRotateProcessor(Options options)
        {
            this.options = options;

            hasRotation = options.CurveX > float.Epsilon || options.CurveY > float.Epsilon;
            hasNoise = options.Noise > float.Epsilon;
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {
            pixel.GetChannelValue(options.NormalX, out var normalX);
            pixel.GetChannelValue(options.NormalY, out var normalY);

            var vx = Math.Clamp(normalX / 127d - 1d, -1d, 1d);
            var vy = Math.Clamp(normalY / 127d - 1d, -1d, 1d);

            var angleX = Math.Asin(vx) / MathEx.Deg2Rad;
            var angleY = Math.Asin(vy) / MathEx.Deg2Rad;

            if (hasRotation) {
                var fx = context.X / (double)context.Width;
                var fy = context.Y / (double)context.Height;
                angleX += options.CurveX * (fx - 0.5d);
                angleY += options.CurveY * (fy - 0.5d);
            }

            if (hasNoise) {
                context.GetNoise(in context.X, out var noiseX, out var noiseY);
                angleX += (noiseX / 127d - 1f) * options.Noise;
                angleY += (noiseY / 127d - 1f) * options.Noise;
            }

            MathEx.Clamp(ref angleX, -90d, 90d);
            MathEx.Clamp(ref angleY, -90f, 90f);

            var sinX = Math.Sin(angleX * MathEx.Deg2Rad);
            var cosX = Math.Cos(angleX * MathEx.Deg2Rad);
            var sinY = Math.Sin(angleY * MathEx.Deg2Rad);
            var cosY = Math.Cos(angleY * MathEx.Deg2Rad);

            Vector3 v;
            v.X = (float)(sinX * cosY);
            v.Y = (float)(sinY * cosX);
            v.Z = (float)(cosX * cosY);
            MathEx.Normalize(ref v);

            if (options.NormalX != ColorChannel.None) {
                MathEx.SaturateFloor(v.X * 0.5f + 0.5f, out normalX);
                pixel.SetChannelValue(options.NormalX, normalX);
            }

            if (options.NormalY != ColorChannel.None) {
                MathEx.SaturateFloor(v.Y * 0.5f + 0.5f, out normalY);
                pixel.SetChannelValue(options.NormalY, normalY);
            }

            if (options.NormalZ != ColorChannel.None) {
                MathEx.SaturateFloor(v.Z, out var normalZ);
                pixel.SetChannelValue(options.NormalZ, normalZ);
            }
        }

        public class Options
        {
            public ColorChannel NormalX = ColorChannel.None;
            public ColorChannel NormalY = ColorChannel.None;
            public ColorChannel NormalZ = ColorChannel.None;
            public float CurveX = 0f;
            public float CurveY = 0f;
            public float Noise = 0f;


            //public bool HasAllMappings()
            //{
            //    if (NormalX == ColorChannel.None) return false;
            //    if (NormalY == ColorChannel.None) return false;
            //    if (NormalZ == ColorChannel.None) return false;
            //    return true;
            //}

            //public bool HasRotations()
            //{
            //    if (CurveX > float.Epsilon) return true;
            //    if (CurveY > float.Epsilon) return true;
            //    if (Noise > float.Epsilon) return true;
            //    return false;
            //}
        }
    }
}
