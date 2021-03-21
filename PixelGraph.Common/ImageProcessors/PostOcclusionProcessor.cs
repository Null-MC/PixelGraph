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
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var albedoPixel = row[x].ToScaledVector4();
                var (fx, fy) = GetTexCoord(in context, in x);
                options.OcclusionSampler.Sample(in fx, in fy, in options.OcclusionMapping.InputColor, out var occlusionPixel);

                if (!options.OcclusionMapping.TryUnmap(ref occlusionPixel, out var occlusionValue))
                    occlusionValue = 0f;

                if (options.EmissiveSampler != null) {
                    options.EmissiveSampler.Sample(in fx, in fy, in options.EmissiveMapping.InputColor, out var emissivePixel);

                    if (options.EmissiveMapping.TryUnmap(ref emissivePixel, out var emissiveValue))
                        occlusionValue = Math.Max(occlusionValue - emissiveValue, 0f);
                }

                var lit = 1f - occlusionValue;
                foreach (var color in options.MappingColors) {
                    albedoPixel.GetChannelValue(in color, out var value);

                    // TODO: currently just applying to mapping as-is
                    // ...but it SHOULD be respecting the full channel unmap>edit>remap workflow
                    value *= lit;

                    albedoPixel.SetChannelValue(in color, in value);
                }

                row[x].FromScaledVector4(albedoPixel);
            }
        }

        public class Options
        {
            public ColorChannel[] MappingColors;

            public ISampler<TOcclusion> OcclusionSampler;
            public TextureChannelMapping OcclusionMapping;

            public ISampler<TEmissive> EmissiveSampler;
            public TextureChannelMapping EmissiveMapping;
        }
    }
}
