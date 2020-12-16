using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OverlayProcessor : PixelProcessor
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            byte value;
            if (options.InputValue.HasValue) {
                value = options.InputValue.Value;
            }
            else {
                var pixelIn = options.Source[context.X, context.Y];
                pixelIn.GetChannelValue(in options.InputColor, out var rawValue);

                if (options.InputMin != 0 || options.InputMax != 255) {
                    // Discard operation if source value outside bounds
                    if (rawValue < options.InputMin || rawValue > options.InputMax) return;

                    // Input: scale to range
                    var offset = rawValue - options.InputMin;
                    var range = options.InputMax - options.InputMin;
                    MathEx.Saturate(offset / (float)range, out value);
                }
                else {
                    value = rawValue;
                }

                // Input: cycle
                MathEx.Cycle(ref value, -options.InputShift);

                // Input: invert
                if (options.InvertInput) value = (byte)(255 - value);

                // Input: power
                if (MathF.Abs(options.InputPower - 1f) > float.Epsilon) {
                    var x = MathF.Pow(value / 255f, 1f / options.InputPower);
                    MathEx.Saturate(in x, out value);
                }
            }

            // convert to float
            var fValue = value / 255f;

            // Common Processing

            if (MathF.Abs(options.Scale - 1f) > float.Epsilon)
                fValue *= options.Scale;

            // Output: power
            if (MathF.Abs(options.OutputPower - 1f) > float.Epsilon) {
                fValue = MathF.Pow(fValue, options.OutputPower);
            }

            // Output: invert
            if (options.InvertOutput) fValue = 1f - fValue;

            // Output: convert to byte and shift
            MathEx.Saturate(in fValue, out value);
            MathEx.Cycle(ref value, options.OutputShift);

            // Output: scale to range
            if (options.OutputMin != 0 || options.OutputMax != 255) {
                var f = value / 255f;
                var range = options.OutputMax - options.OutputMin;
                value = MathEx.Clamp(options.OutputMin + (int) (f * range));
            }

            pixelOut.SetChannelValue(options.OutputColor, value);
        }

        public class Options
        {
            public Image<Rgba32> Source;

            public bool InvertInput;
            public ColorChannel InputColor;
            public byte? InputValue;
            public byte InputMin;
            public byte InputMax;
            public short InputShift;
            public float InputPower;

            public bool InvertOutput;
            public ColorChannel OutputColor;
            public byte OutputMin;
            public byte OutputMax;
            public int OutputShift;
            public float OutputPower;

            public float Scale = 1f;
        }
    }
}
