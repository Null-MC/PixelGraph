using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class PostOcclusionProcessor<TOcclusion, TEmissive> : PixelRowProcessor
        where TOcclusion : unmanaged, IPixel<TOcclusion>
        where TEmissive : unmanaged, IPixel<TEmissive>
    {
        private readonly Options options;


        public PostOcclusionProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TSource>(in PixelRowContext context, Span<TSource> row)
        {
            byte occlusionPixel;
            float fx, fy, occlusionValue;
            var colorCount = options.MappingColors.Length;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var albedoPixel = row[x].ToScaledVector4();
                GetTexCoord(in context, in x, out fx, out fy);
                options.OcclusionSampler.Sample(in fx, in fy, in options.OcclusionInputColor, out occlusionPixel);

                if (!options.OcclusionMapping.TryUnmap(in occlusionPixel, out occlusionValue))
                    occlusionValue = 0f;

                occlusionValue *= options.OcclusionMapping.OutputScale;

                if (options.EmissiveSampler != null) {
                    options.EmissiveSampler.Sample(in fx, in fy, in options.EmissiveInputColor, out var emissivePixel);

                    if (options.EmissiveMapping.TryUnmap(in emissivePixel, out var emissiveValue))
                        occlusionValue = MathF.Max(occlusionValue - emissiveValue, 0f);
                }

                var lit = 1f - occlusionValue;
                for (var c = 0; c < colorCount; c++) {
                    albedoPixel.GetChannelValue(in options.MappingColors[c], out var value);

                    // TODO: currently just applying to mapping as-is
                    // ...but it SHOULD be respecting the full channel unmap>edit>remap workflow
                    value *= lit;

                    albedoPixel.SetChannelValue(in options.MappingColors[c], in value);
                }

                row[x].FromScaledVector4(albedoPixel);
            }
        }

        public class Options
        {
            public ColorChannel[] MappingColors;

            public ISampler<TOcclusion> OcclusionSampler;
            public ColorChannel OcclusionInputColor;
            public PixelMapping OcclusionMapping;

            public ISampler<TEmissive> EmissiveSampler;
            public ColorChannel EmissiveInputColor;
            public PixelMapping EmissiveMapping;
        }
    }
}
