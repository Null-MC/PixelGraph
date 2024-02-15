using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using Color = System.Drawing.Color;

namespace PixelGraph.Common.ImageProcessors;

internal class ItemProcessor<TOcclusion, TEmissive> : PixelRowProcessor
    where TOcclusion : unmanaged, IPixel<TOcclusion>
    where TEmissive : unmanaged, IPixel<TEmissive>
{
    private readonly Options options;
    private readonly Vector3 ambientColor, lightColor;


    public ItemProcessor(in Options options)
    {
        this.options = options;

        ambientColor.X = options.AmbientColor.R / 255f;
        ambientColor.Y = options.AmbientColor.G / 255f;
        ambientColor.Z = options.AmbientColor.B / 255f;

        lightColor.X = options.LightColor.R / 255f;
        lightColor.Y = options.LightColor.G / 255f;
        lightColor.Z = options.LightColor.B / 255f;
    }

    protected override void ProcessRow<TSource>(in PixelRowContext context, Span<TSource> row)
    {
        GetTexCoordY(in context, out var fy);
        var normalRowSampler = options.NormalSampler?.ForRow(in fy);
        var occlusionRowSampler = options.OcclusionSampler?.ForRow(in fy);
        var emissiveRowSampler = options.EmissiveSampler?.ForRow(in fy);

        for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
            var albedoPixel = row[x].ToScaledVector4();
            GetTexCoord(in context, in x, out var fx, out fy);

            var occlusionValue = 0f;
            if (occlusionRowSampler != null) {
                occlusionRowSampler.SampleScaled(in fx, in fy, in options.OcclusionInputColor, out var occlusionPixel);
                options.OcclusionMapping.TryUnmap(in occlusionPixel, out occlusionValue);
            }

            var litNormal = 1f;
            if (normalRowSampler != null) {
                normalRowSampler.SampleScaled(in fx, in fy, out var normalPixel);

                Vector3 normal;
                normal.X = normalPixel.X * 2f - 1f;
                normal.Y = normalPixel.Y * 2f - 1f;
                normal.Z = normalPixel.Z;
                MathEx.Normalize(ref normal);

                litNormal = Vector3.Dot(normal, options.LightDirection);
                //litNormal = litNormal * (1f - options.Ambient);
            }

            var emissiveValue = 0f;
            emissiveRowSampler?.SampleScaled(in fx, in fy, in options.EmissiveColor, out emissiveValue);

            var lit = ambientColor * (1f - occlusionValue);
            lit.Add(lightColor * litNormal);
            lit.Add(in emissiveValue);

            //var lit = MathF.Min(litNormal, 1f - occlusionValue);
            //lit = MathF.Max(lit, emissiveValue);

            albedoPixel.X *= lit.X;
            albedoPixel.Y *= lit.Y;
            albedoPixel.Z *= lit.Z;
                
            row[x].FromScaledVector4(albedoPixel);
        }
    }

    public class Options
    {
        public Color AmbientColor = Color.FromArgb(76, 76, 76);
        public Color LightColor = Color.FromArgb(200, 200, 200);
        public Vector3 LightDirection = Vector3.UnitZ;

        public ISampler<Rgb24>? NormalSampler;

        public ISampler<TOcclusion>? OcclusionSampler;
        public ColorChannel OcclusionInputColor;
        public PixelMapping OcclusionMapping;

        public ISampler<TEmissive>? EmissiveSampler;
        public ColorChannel EmissiveColor;
    }
}
