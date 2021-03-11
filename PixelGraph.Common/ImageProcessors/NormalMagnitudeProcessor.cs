using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalMagnitudeProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public NormalMagnitudeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var magRow = options.MagSource.GetPixelRowSpan(context.Y);
            var pixelMag = new Rgba32();

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var normal = row[x].ToScaledVector4();
                magRow[x].ToRgba32(ref pixelMag);

                pixelMag.GetChannelValue(in options.InputChannel, out var pixelValue);

                // convert mag pixel-space to value-space

                // Discard operation if source value outside bounds
                if (pixelValue < options.InputRangeMin || pixelValue > options.InputRangeMax) return;

                // Input: cycle
                if (options.InputShift != 0) MathEx.Cycle(ref pixelValue, -options.InputShift, in options.InputRangeMin, in options.InputRangeMax);

                // Convert from pixel-space to value-space
                var pixelInputRange = options.InputRangeMax - options.InputRangeMin;
                var valueInputRange = options.InputMaxValue - options.InputMinValue;
                var inputScale = (double)valueInputRange / pixelInputRange;

                var value = options.InputMinValue + (pixelValue - options.InputRangeMin) * inputScale;

                // Input: invert
                if (options.InputInvert) MathEx.Invert(ref value, options.InputMinValue, options.InputMaxValue);

                // Input: power
                if (!options.InputPower.Equal(1f))
                    value = Math.Pow(value, 1d / options.InputPower);

                // Common operations

                value *= options.Scale;

                if (!options.OutputPower.Equal(1f))
                    value = Math.Pow(value, options.OutputPower);

                if (options.OutputInvert) MathEx.Invert(ref value, options.OutputMinValue, options.OutputMaxValue);

                // convert from value-space to pixel-space
                var valueRange = options.OutputMaxValue - options.OutputMinValue;
                var pixelRange = options.OutputRangeMax - options.OutputRangeMin;
                var outputScale = pixelRange / (double)valueRange;

                var finalValue = options.OutputRangeMin + (value - options.OutputMinValue) * outputScale;
                pixelValue = MathEx.ClampRound(finalValue, options.OutputRangeMin, options.OutputRangeMax);

                normal *= pixelValue;
                row[x].FromScaledVector4(normal);
            }
        }

        public class Options
        {
            public Image<TPixel> MagSource;
            public float Scale;

            public ColorChannel InputChannel;
            public float InputMinValue;
            public float InputMaxValue;
            public byte InputRangeMin;
            public byte InputRangeMax;
            public int InputShift;
            public float InputPower;
            public bool InputInvert;

            public float OutputMinValue;
            public float OutputMaxValue;
            public byte OutputRangeMin;
            public byte OutputRangeMax;
            public int OutputShift;
            public float OutputPower;
            public bool OutputInvert;


            public void ApplyInputChannel(ResourcePackChannelProperties channel)
            {
                InputChannel = channel.Color ?? ColorChannel.None;
                InputMinValue = (float?)channel.MinValue ?? 0f;
                InputMaxValue = (float?)channel.MaxValue ?? 1f;
                InputRangeMin = channel.RangeMin ?? 0;
                InputRangeMax = channel.RangeMax ?? 255;
                InputShift = channel.Shift ?? 0;
                InputPower = (float?) channel.Power ?? 1f;
                InputInvert = channel.Invert ?? false;
            }

            public void ApplyOutputChannel(ResourcePackChannelProperties channel)
            {
                OutputMinValue = (float?)channel.MinValue ?? 0f;
                OutputMaxValue = (float?)channel.MaxValue ?? 1f;
                OutputRangeMin = channel.RangeMin ?? 0;
                OutputRangeMax = channel.RangeMax ?? 255;
                OutputShift = channel.Shift ?? 0;
                OutputPower = (float?) channel.Power ?? 1f;
                OutputInvert = channel.Invert ?? false;
            }
        }
    }
}
