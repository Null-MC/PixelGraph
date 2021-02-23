using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ItemProcessor<TNormal, TOcclusion, TEmissive> : PixelRowProcessor
        where TNormal : unmanaged, IPixel<TNormal>
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
            Span<TNormal> normalRow = null;
            if (options.NormalImage != null) {
                normalRow = options.NormalImage.GetPixelRowSpan(context.Y);
            }

            Span<TOcclusion> occlusionRow = null;
            if (options.OcclusionImage != null) {
                occlusionRow = options.OcclusionImage.GetPixelRowSpan(context.Y);
            }

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var albedoPixel = row[x].ToScaledVector4();
                var fx = (float)x / context.Bounds.Width;
                var fy = (float)context.Y / context.Bounds.Height;

                var litOcclusion = 1f;
                if (options.OcclusionImage != null) {
                    var occlusionPixel = occlusionRow[x].ToScaledVector4();
                    litOcclusion = occlusionPixel.X;
                }

                var litNormal = 1f;
                if (options.NormalImage != null) {
                    var normal = normalRow[x].ToScaledVector4();
                    normal.X = normal.X * 2f - 1f;
                    normal.Y = normal.Y * 2f - 1f;
                    MathEx.Normalize(ref normal);

                    litNormal = Vector4.Dot(normal, options.LightDirection);
                }

                var emissiveValue = 0f;
                options.EmissiveSampler?.SampleScaled(in fx, in fy, in options.EmissiveColor, out emissiveValue);

                var lit = MathF.Min(litNormal, litOcclusion);
                lit = MathF.Max(lit, emissiveValue);

                albedoPixel.X *= lit;
                albedoPixel.Y *= lit;
                albedoPixel.Z *= lit;
                
                row[x].FromScaledVector4(albedoPixel);
            }
        }

        public class Options
        {
            public Image<TNormal> NormalImage;
            public Image<TOcclusion> OcclusionImage;
            public ISampler<TEmissive> EmissiveSampler;
            public ColorChannel EmissiveColor;
            public Vector4 LightDirection = Vector4.UnitZ;
        }
    }
}
