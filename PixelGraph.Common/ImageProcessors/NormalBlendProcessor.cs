using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalBlendProcessor : PixelRowProcessor
    {
        private readonly Options options;


        public NormalBlendProcessor(Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixelSrc = new Rgba32();
            var pixelBlend = new Rgba32();

            var blendRow = options.BlendImage.GetPixelRowSpan(context.Y);

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixelSrc);
                blendRow[x].ToRgba32(ref pixelBlend);

                // Convert Source from Normal to Vector
                pixelSrc.GetChannelValueScaledF(ColorChannel.Red, out var normalSrcX);
                pixelSrc.GetChannelValueScaledF(ColorChannel.Green, out var normalSrcY);

                var vxSrc = Math.Clamp(normalSrcX * 2f - 1f, -1f, 1f);
                var vySrc = Math.Clamp(normalSrcY * 2f - 1f, -1f, 1f);
                var angleSrcX = MathF.Asin(vxSrc) / MathEx.Deg2Rad;
                var angleSrcY = MathF.Asin(vySrc) / MathEx.Deg2Rad;

                // Convert Blend from Normal to Vector
                pixelBlend.GetChannelValueScaledF(ColorChannel.Red, out var normalBlendX);
                pixelBlend.GetChannelValueScaledF(ColorChannel.Green, out var normalBlendY);

                var vxBlend = Math.Clamp(normalBlendX * 2f - 1f, -1f, 1f);
                var vyBlend = Math.Clamp(normalBlendY * 2f - 1f, -1f, 1f);
                var angleBlendX = MathF.Asin(vxBlend) / MathEx.Deg2Rad;
                var angleBlendY = MathF.Asin(vyBlend) / MathEx.Deg2Rad;

                // TODO: MATH
                //var angleFinalX = angleSrcX + angleBlendX;
                //var angleFinalY = angleSrcY + angleBlendY;

                var blend = options.Blend;
                MathEx.Clamp(ref blend, 0f, 1f);
                MathEx.Lerp(angleSrcX, angleBlendX, blend, out var angleFinalX);
                MathEx.Lerp(angleSrcY, angleBlendY, blend, out var angleFinalY);

                // Convert from Vector to Normal
                MathEx.Clamp(ref angleFinalX, -90f, 90f);
                MathEx.Clamp(ref angleFinalY, -90f, 90f);

                var sinX = MathF.Sin(angleFinalX * MathEx.Deg2Rad);
                var cosX = MathF.Cos(angleFinalX * MathEx.Deg2Rad);
                var sinY = MathF.Sin(angleFinalY * MathEx.Deg2Rad);
                var cosY = MathF.Cos(angleFinalY * MathEx.Deg2Rad);

                Vector3 v;
                v.X = sinX * cosY;
                v.Y = sinY * cosX;
                v.Z = cosX * cosY;
                MathEx.Normalize(ref v);

                pixelSrc.SetChannelValueScaledF(ColorChannel.Red, v.X * 0.5f + 0.5f);
                pixelSrc.SetChannelValueScaledF(ColorChannel.Green, v.Y * 0.5f + 0.5f);
                pixelSrc.SetChannelValueScaledF(ColorChannel.Blue, v.Z);
                row[x].FromRgba32(pixelSrc);
            }
        }

        public class Options
        {
            public Image<Rgb24> BlendImage;
            public float Blend = 0f;
        }
    }
}
