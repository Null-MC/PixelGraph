using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

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
            float fx, fy, valueIn, value, ValueOut;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var normal = row[x].ToScaledVector4();

                GetTexCoord(in context, in x, out fx, out fy);
                options.MagSampler.SampleScaled(in fx, in fy, in options.Mapping.InputColor, out valueIn);
                if (!options.Mapping.TryUnmap(in valueIn, out value)) continue;

                if (!options.Mapping.ValueShift.Equal(0f))
                    value += options.Mapping.ValueShift;

                if (!options.Mapping.ValueScale.Equal(1f))
                    value *= options.Mapping.ValueScale;

                options.Mapping.Map(ref value, out ValueOut);
                if (ValueOut.Equal(1f)) continue;

                row[x].FromScaledVector4(normal * ValueOut);
            }
        }

        public class Options
        {
            public ISampler<TPixel> MagSampler;
            public TextureChannelMapping Mapping;
            public float Scale;
        }
    }
}
