using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class PostOcclusionProcessor<TOcclusion> : PixelRowProcessor
        where TOcclusion : unmanaged, IPixel<TOcclusion>
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
                var fx = (x + HalfPixel) / context.Bounds.Width;
                var fy = (context.Y + HalfPixel) / context.Bounds.Height;

                options.OcclusionSampler.SampleScaled(in fx, in fy, in options.OcclusionColor, out var occlusionValue);

                foreach (var color in options.MappingColors) {
                    albedoPixel.GetChannelValue(in color, out var value);

                    // TODO: currently just applying to mapping as-is
                    // ...but it SHOULD be respecting the full channel unmap>edit>remap workflow
                    value *= occlusionValue;

                    albedoPixel.SetChannelValue(in color, in value);
                }

                albedoPixel.X *= occlusionValue;
                albedoPixel.Y *= occlusionValue;
                albedoPixel.Z *= occlusionValue;
                
                row[x].FromScaledVector4(albedoPixel);
            }
        }

        public class Options
        {
            public ColorChannel[] MappingColors;

            public ISampler<TOcclusion> OcclusionSampler;
            public ColorChannel OcclusionColor;
        }
    }
}
