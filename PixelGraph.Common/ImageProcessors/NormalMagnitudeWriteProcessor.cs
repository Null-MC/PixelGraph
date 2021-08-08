using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalMagnitudeWriteProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public NormalMagnitudeWriteProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            Vector3 normal;
            float fx, fy, valueIn, value, ValueOut;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                GetTexCoord(in context, in x, out fx, out fy);
                options.MagSampler.SampleScaled(in fx, in fy, in options.InputColor, out valueIn);
                if (!options.Mapping.TryUnmap(in valueIn, out value)) continue;

                if (!options.ValueShift.NearEqual(0f))
                    value += options.ValueShift;

                if (!options.Mapping.OutputValueScale.NearEqual(1f))
                    value *= options.Mapping.OutputValueScale;

                options.Mapping.Map(ref value, out ValueOut);
                if (ValueOut.NearEqual(1f)) continue;


                var normalPixel = row[x].ToScaledVector4();

                normal.X = normalPixel.X * 2f - 1f;
                normal.Y = normalPixel.Y * 2f - 1f;
                normal.Z = normalPixel.Z * 2f - 1f;

                MathEx.Normalize(ref normal);
                normal *= ValueOut;

                normalPixel.X = normal.X * 0.5f + 0.5f;
                normalPixel.Y = normal.Y * 0.5f + 0.5f;
                normalPixel.Z = normal.Z * 0.5f + 0.5f;

                row[x].FromScaledVector4(normalPixel);
            }
        }

        public class Options
        {
            public ColorChannel InputColor;
            public ISampler<TPixel> MagSampler;
            public PixelMapping Mapping;
            public float ValueShift;
            public float Scale;
        }
    }
}
