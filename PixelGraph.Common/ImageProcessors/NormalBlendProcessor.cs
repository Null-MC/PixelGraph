using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
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
            var pixelOut = new Rgb24();
            var vectorFinal = new Vector3();

            var rowHigh = options.HighFreqNormalImage.DangerousGetPixelRowMemory(context.Y).Span;
            var rowLow = options.LowFreqNormalImage.DangerousGetPixelRowMemory(context.Y).Span;
            var rowVariance = options.VarianceImage.DangerousGetPixelRowMemory(context.Y).Span;

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                rowVariance[x].GetChannelValueScaledF(out var variance);

                rowHigh[x].ToNormalVector(out var vectorHigh);
                vectorHigh.ToEuler(out var angleHigh);

                rowLow[x].ToNormalVector(out var vectorLow);
                vectorLow.ToEuler(out var angleLow);

                MathEx.Lerp(in angleLow, in angleHigh, in variance, out var angleFinal);

                MathEx.Clamp(ref angleFinal.X, -90f, 90f);
                MathEx.Clamp(ref angleFinal.Y, -90f, 90f);

                vectorFinal.FromEuler(in angleFinal);
                pixelOut.FromNormalVector(in vectorFinal);
                row[x].FromRgb24(pixelOut);
            }
        }

        public class Options
        {
            public Image<Rgb24> HighFreqNormalImage;
            public Image<Rgb24> LowFreqNormalImage;
            public Image<L8> VarianceImage;
        }
    }
}
