using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OverlayProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixelOut = new Rgba32();
            double value;

            var mappingCount = options.Mappings.Length;

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixelOut);

                for (var i = 0; i < mappingCount; i++) {
                    var mapping = options.Mappings[i];
                    pixelOut.GetChannelValue(in mapping.OutputColor, out var existingValue);

                    if (existingValue != 0) {
                        if (existingValue < mapping.OutputRangeMin || existingValue > mapping.OutputRangeMax) continue;
                    }

                    if (mapping.InputValue.HasValue)
                        value = (double)mapping.InputValue.Value;
                    else {
                        byte pixelValue = 0;
                        if (options.SamplerMap.TryGetValue(mapping.InputColor, out var sampler)) {
                            var fx = (x - context.Bounds.X + HalfPixel) / context.Bounds.Width;
                            var fy = (context.Y - context.Bounds.Y + HalfPixel) / context.Bounds.Height;
                            sampler.Sample(fx, fy, in mapping.InputColor, out pixelValue);
                        }

                        // Discard operation if source value outside bounds
                        if (pixelValue < mapping.InputRangeMin || pixelValue > mapping.InputRangeMax) continue;

                        // Input: cycle
                        if (mapping.InputShift != 0) MathEx.Cycle(ref pixelValue, -mapping.InputShift, in mapping.InputRangeMin, in mapping.InputRangeMax);

                        value = pixelValue - mapping.InputRangeMin;

                        // Convert from pixel-space to value-space
                        var pixelInputRange = mapping.InputRangeMax - mapping.InputRangeMin;

                        if (pixelInputRange > 0) {
                            var valueInputRange = mapping.InputMaxValue - mapping.InputMinValue;
                            if (!valueInputRange.Equal(pixelInputRange))
                                value *= valueInputRange / pixelInputRange;
                        }

                        value += mapping.InputMinValue;

                        if (mapping.InputInverted) MathEx.Invert(ref value, mapping.InputMinValue, mapping.InputMaxValue);

                        if (!mapping.InputPower.Equal(1d))
                            value = Math.Pow(value, 1d / mapping.InputPower);
                    }

                    // Common Processing
                    if (!mapping.ValueShift.Equal(0f))
                        value += mapping.ValueShift;

                    if (!mapping.ValueScale.Equal(1f))
                        value *= mapping.ValueScale;

                    if (!mapping.OutputPower.Equal(1d))
                        value = Math.Pow(value, mapping.OutputPower);

                    if (mapping.OutputInverted) MathEx.Invert(ref value, mapping.InputMinValue, mapping.OutputMaxValue);

                    // convert from value-space to pixel-space
                    var valueRange = mapping.OutputMaxValue - mapping.OutputMinValue;
                    var pixelRange = mapping.OutputRangeMax - mapping.OutputRangeMin;

                    var valueOut = value - mapping.OutputMinValue;

                    if (pixelRange == 0 || valueRange == 0)
                        valueOut = 0;
                    else if (!valueRange.Equal(pixelRange))
                        valueOut *= pixelRange / valueRange;

                    valueOut += mapping.OutputRangeMin;

                    var finalValue = MathEx.ClampRound(valueOut, mapping.OutputRangeMin, mapping.OutputRangeMax);

                    if (mapping.OutputShift != 0)
                        MathEx.Cycle(ref finalValue, in mapping.OutputShift, in mapping.OutputRangeMin, in mapping.OutputRangeMax);

                    if (options.IsGrayscale) {
                        pixelOut.R = finalValue;
                        pixelOut.G = finalValue;
                        pixelOut.B = finalValue;
                    }
                    else {
                        pixelOut.SetChannelValue(mapping.OutputColor, finalValue);
                    }
                }

                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public TextureChannelMapping[] Mappings;
            public Dictionary<ColorChannel, ISampler<TPixel>> SamplerMap;
            public bool IsGrayscale;
        }
    }
}
