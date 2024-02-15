using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.ImageProcessors;

internal class OverlayProcessor<TPixel>(OverlayProcessor<TPixel>.Options options) : PixelRowProcessor
    where TPixel : unmanaged, IPixel<TPixel>
{
    protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
    {
        ArgumentNullException.ThrowIfNull(options.Samplers);

        GetTexCoordY(in context, out var rfy);
        var samplerCount = options.Samplers.Length;

        var rowSamplers = new IRowSampler[samplerCount];
        for (var i = 0; i < samplerCount; i++)
            rowSamplers[i] = options.Samplers[i].Sampler?.ForRow(rfy) ?? throw new ApplicationException("Sampler is undefined!");

        for (var x = 0; x < context.Bounds.Width; x++) {
            var pixelOut = row[x].ToScaledVector4();

            for (var i = 0; i < samplerCount; i++) {
                var samplerOptions = options.Samplers[i];
                var mapping = samplerOptions.PixelMap;

                GetTexCoord(in context, context.Bounds.Left + x, out var fx, out var fy);
                rowSamplers[i].SampleScaled(fx, fy, in samplerOptions.InputColor, out var pixelValue);

                if (!mapping.TryUnmap(in pixelValue, out var value)) continue;

                if (!ProcessConversions(samplerOptions.PixelMap, ref value)) continue;

                if (!mapping.TryMap(ref value, out var finalValue)) continue;

                if (mapping.OutputClipValue.HasValue && value.NearEqual(mapping.OutputClipValue.Value / 255f)) continue;

                if (options.IsGrayscale) {
                    pixelOut.X = pixelOut.Y = pixelOut.Z = finalValue;
                }
                else {
                    pixelOut.SetChannelValue(samplerOptions.OutputColor, finalValue);
                }
            }

            row[x].FromScaledVector4(pixelOut);
        }
    }

    private static bool ProcessConversions(PixelMapping map, ref float value)
    {
        if (map.Convert_HcmToMetal) {
            value = value >= 230f - float.Epsilon
                ? map.OutputMaxValue
                : map.OutputMinValue;
        }
        else if (map.Convert_MetalToHcm) {
            var threshold = (map.InputMinValue + map.InputMaxValue) * 0.5f;
            if (value < threshold) return false;

            value = map.OutputMaxValue;
        }
        //else if (map.Convert_SmoothToSpecular) {
        //    value = 2f / MathF.Pow(1f - value, 4) - 2f;
        //}
        //else if (map.Convert_SpecularToSmooth) {
        //    value = 1f - MathF.Pow((value + 2f) * 2f, 0.25f);
        //}
        //else if (map.Convert_RoughToSpecular) {
        //    value = 2f / MathF.Pow(value, 4) - 2f;
        //}
        //else if (map.Convert_SpecularToRough) {
        //    value = MathF.Pow((value + 2f) * 2f, 0.25f);
        //}

        return true;
    }

    public class Options
    {
        public SamplerOptions[]? Samplers;
        public bool IsGrayscale;
    }

    public class SamplerOptions
    {
        public PixelMapping PixelMap;
        public ISampler<TPixel>? Sampler;
        public ColorChannel InputColor;
        public ColorChannel OutputColor;
    }
}