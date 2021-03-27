using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ItemProcessor<TOcclusion, TEmissive> : PixelRowProcessor
        where TOcclusion : unmanaged, IPixel<TOcclusion>
        where TEmissive : unmanaged, IPixel<TEmissive>
    {
        private readonly Options options;


        public ItemProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TSource>(in PixelRowContext context, Span<TSource> row)
        {
            float fx, fy, occlusionValue;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var albedoPixel = row[x].ToScaledVector4();
                GetTexCoord(in context, in x, out fx, out fy);

                occlusionValue = 1f;
                options.OcclusionSampler?.SampleScaled(in fx, in fy, in options.OcclusionColor, out occlusionValue);

                var litNormal = 1f;
                if (options.NormalSampler != null) {
                    options.NormalSampler.SampleScaled(in fx, in fy, out var normal);
                    normal.X = normal.X * 2f - 1f;
                    normal.Y = normal.Y * 2f - 1f;
                    MathEx.Normalize(ref normal);

                    litNormal = Vector4.Dot(normal, options.LightDirection);
                }

                var emissiveValue = 0f;
                options.EmissiveSampler?.SampleScaled(in fx, in fy, in options.EmissiveColor, out emissiveValue);

                var lit = MathF.Min(litNormal, occlusionValue);
                lit = MathF.Max(lit, emissiveValue);

                albedoPixel.X *= lit;
                albedoPixel.Y *= lit;
                albedoPixel.Z *= lit;
                
                row[x].FromScaledVector4(albedoPixel);
            }
        }

        public class Options
        {
            public Vector4 LightDirection = Vector4.UnitZ;

            public ISampler<Rgb24> NormalSampler;

            public ISampler<TOcclusion> OcclusionSampler;
            public ColorChannel OcclusionColor;

            public ISampler<TEmissive> EmissiveSampler;
            public ColorChannel EmissiveColor;
        }
    }
}
