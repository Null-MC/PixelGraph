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

                // Discard operation if source value outside bounds
                if (rawValue < options.InputMin || rawValue > options.InputMax) return;

                var offset = rawValue - options.InputMin;
                var range = options.InputMax - options.InputMin;
                MathEx.Saturate(offset / (float)range, out value);

                // Input Processing

                MathEx.Cycle(ref value, -options.InputShift);

                if (options.InvertInput) value = (byte)(255 - value);

                if (MathF.Abs(options.InputPower - 1f) > float.Epsilon) {
                    var x = MathF.Pow(value / 255f, 1f / options.InputPower);
                    MathEx.Saturate(in x, out value);
                }
            }

            var fValue = value / 255f;

            // Common Processing

            if (MathF.Abs(options.Scale - 1f) > float.Epsilon)
                fValue *= options.Scale;

            // Output Processing

            if (MathF.Abs(options.OutputPower - 1f) > float.Epsilon) {
                fValue = MathF.Pow(fValue, options.OutputPower);
            }

            if (options.InvertOutput) fValue = 1f - fValue;

            MathEx.Saturate(in fValue, out value);
            MathEx.Cycle(ref value, options.OutputShift);
            pixelOut.SetChannelValue(options.OutputColor, value);

            // TODO: range
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
