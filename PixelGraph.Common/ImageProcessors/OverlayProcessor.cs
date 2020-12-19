using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OverlayProcessor<TPixel> : PixelProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var mapping = options.Mapping;

            byte value;
            if (mapping.InputValue.HasValue) {
                value = mapping.InputValue.Value;
            }
            else {
                var pixelIn = new Rgba32();
                options.Source[context.X, context.Y].ToRgba32(ref pixelIn);
                pixelIn.GetChannelValue(in mapping.InputColor, out var rawValue);

                if (mapping.InputMin != 0 || mapping.InputMax != 255) {
                    // Discard operation if source value outside bounds
                    if (rawValue < mapping.InputMin || rawValue > mapping.InputMax) return;

                    // Input: scale to range
                    var offset = rawValue - mapping.InputMin;
                    var range = mapping.InputMax - mapping.InputMin;
                    MathEx.Saturate(offset / (float)range, out value);
                }
                else {
                    value = rawValue;
                }

                // Input: cycle
                MathEx.Cycle(ref value, -mapping.InputShift);

                // Input: invert
                if (mapping.InvertInput) value = (byte)(255 - value);

                // Input: power
                if (MathF.Abs(mapping.InputPower - 1f) > float.Epsilon) {
                    var x = MathF.Pow(value / 255f, 1f / mapping.InputPower);
                    MathEx.Saturate(in x, out value);
                }
            }

            // convert to float
            var fValue = value / 255f;

            // Common Processing

            if (MathF.Abs(options.Scale - 1f) > float.Epsilon)
                fValue *= options.Scale;

            // Output: power
            if (MathF.Abs(mapping.OutputPower - 1f) > float.Epsilon) {
                fValue = MathF.Pow(fValue, mapping.OutputPower);
            }

            // Output: invert
            if (mapping.InvertOutput) fValue = 1f - fValue;

            // Output: convert to byte and shift
            MathEx.Saturate(in fValue, out value);
            MathEx.Cycle(ref value, mapping.OutputShift);

            // Output: scale to range
            if (mapping.OutputMin != 0 || mapping.OutputMax != 255) {
                var f = value / 255f;
                var range = mapping.OutputMax - mapping.OutputMin;
                value = MathEx.Clamp(mapping.OutputMin + (int) (f * range));
            }

            pixelOut.SetChannelValue(mapping.OutputColor, value);
        }

        public class Options
        {
            public Image<TPixel> Source;
            public TextureChannelMapping Mapping;
            public float Scale = 1f;
        }
    }
}
