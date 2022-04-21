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
            double fx, fy;
            float occlusionValue;
            Vector4 albedoPixel;

            GetTexCoordY(in context, out fy);
            var normalRowSampler = options.NormalSampler?.ForRow(in fy);
            var occlusionRowSampler = options.OcclusionSampler?.ForRow(in fy);
            var emissiveRowSampler = options.EmissiveSampler?.ForRow(in fy);

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                albedoPixel = row[x].ToScaledVector4();
                GetTexCoord(in context, in x, out fx, out fy);

                occlusionValue = 0f;
                if (occlusionRowSampler != null) {
                    occlusionRowSampler.SampleScaled(in fx, in fy, in options.OcclusionInputColor, out var occlusionPixel);
                    options.OcclusionMapping.TryUnmap(in occlusionPixel, out occlusionValue);
                }

                var litNormal = 1f;
                if (normalRowSampler != null) {
                    normalRowSampler.SampleScaled(in fx, in fy, out var normal);
                    normal.X = normal.X * 2f - 1f;
                    normal.Y = normal.Y * 2f - 1f;
                    normal.Z = normal.Z * 2f - 1f;
                    MathEx.Normalize(ref normal);

                    litNormal = Vector4.Dot(normal, options.LightDirection);
                    litNormal = options.Ambient + litNormal * (1f - options.Ambient);
                }

                var emissiveValue = 0f;
                emissiveRowSampler?.SampleScaled(in fx, in fy, in options.EmissiveColor, out emissiveValue);

                var lit = MathF.Min(litNormal, 1f - occlusionValue);
                lit = MathF.Max(lit, emissiveValue);

                albedoPixel.X *= lit;
                albedoPixel.Y *= lit;
                albedoPixel.Z *= lit;
                
                row[x].FromScaledVector4(albedoPixel);
            }
        }

        public class Options
        {
            public float Ambient = 0.3f;
            public Vector4 LightDirection = Vector4.UnitZ;

            public ISampler<Rgb24> NormalSampler;

            public ISampler<TOcclusion> OcclusionSampler;
            public ColorChannel OcclusionInputColor;
            public PixelMapping OcclusionMapping;

            public ISampler<TEmissive> EmissiveSampler;
            public ColorChannel EmissiveColor;
        }
    }
}
