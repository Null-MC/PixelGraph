using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

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
            var pixelIn = new Rgba32();
            var pixelOut = new Rgba32();
            float fValue;

            var overlayRow = options.Source.GetPixelRowSpan(context.Y);

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                overlayRow[x].ToRgba32(ref pixelIn);
                row[x].ToRgba32(ref pixelOut);

                foreach (var mapping in options.Mappings) {
                    pixelOut.GetChannelValue(in mapping.OutputColor, out var existingValue);

                    if (existingValue != 0 && (existingValue < mapping.OutputMin || existingValue > mapping.OutputMax)) continue;
                    

                    if (mapping.InputValue.HasValue)
                        fValue = mapping.InputValue.Value / 255f;
                    else {
                        pixelIn.GetChannelValue(in mapping.InputColor, out var rawValue);

                        if (mapping.InputMin != 0 || mapping.InputMax != 255) {
                            // Discard operation if source value outside bounds
                            if (rawValue < mapping.InputMin || rawValue > mapping.InputMax) return;

                            // Input: scale to range
                            var offset = rawValue - mapping.InputMin;
                            var range = mapping.InputMax - mapping.InputMin;
                            fValue = offset / (float) range;
                        }
                        else {
                            //value = rawValue;
                            fValue = rawValue / 255f;
                        }

                        // Input: cycle
                        if (mapping.InputShift != 0)
                            MathEx.Cycle(ref fValue, -mapping.InputShift);

                        // Input: invert
                        if (mapping.InvertInput) MathEx.Invert(ref fValue);

                        // Input: power
                        if (MathF.Abs(mapping.InputPower - 1f) > float.Epsilon)
                            fValue = MathF.Pow(fValue, 1f / mapping.InputPower);
                    }

                    // Common Processing

                    // Shift
                    if (mapping.Shift != 0)
                        fValue += mapping.Shift / 255f;

                    // Scale
                    if (!mapping.Scale.Equal(1f))
                        fValue *= mapping.Scale;

                    // Output: power
                    if (!mapping.OutputPower.Equal(1f))
                        fValue = MathF.Pow(fValue, mapping.OutputPower);

                    // Output: invert
                    if (mapping.InvertOutput) MathEx.Invert(ref fValue);

                    // Output: shift
                    if (mapping.OutputShift != 0)
                        MathEx.Cycle(ref fValue, mapping.OutputShift);

                    // Output: scale to range
                    if (mapping.OutputMin != 0 || mapping.OutputMax != 255) {
                        var range = mapping.OutputMax - mapping.OutputMin;
                        var value = MathEx.Clamp(mapping.OutputMin + (int) (fValue * range));
                        pixelOut.SetChannelValue(mapping.OutputColor, value);
                    }
                    else {
                        pixelOut.SetChannelValueScaled(mapping.OutputColor, fValue);
                    }
                }

                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public Image<TPixel> Source;
            public TextureChannelMapping[] Mappings;
        }
    }
}
