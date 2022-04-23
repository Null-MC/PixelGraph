using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class BedrockGrassOpacityProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public BedrockGrassOpacityProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixelOut = new Rgba32();
            var overlayPixel = new Rgba32();

            GetTexCoordY(in context, out var rfy);
            var overlayRowSampler = options.SamplerOverlay.ForRow(in rfy);
            var opacityRowSampler = options.SamplerOpacity?.ForRow(in rfy);

            double fx, fy;
            byte opacityValue;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixelOut);
                GetTexCoord(in context, in x, out fx, out fy);

                if (opacityRowSampler != null) {
                    opacityRowSampler.Sample(fx, fy, ColorChannel.Alpha, out opacityValue);
                }
                else {
                    opacityValue = pixelOut.A;
                }

                if (opacityValue > options.Threshold) continue;

                overlayRowSampler.Sample(fx, fy, ref overlayPixel);

                // TODO: this should probably use the profile mappings instead of assuming
                //if (!mapping.TryUnmap(in pixelValue, out value)) continue;
                //if (!mapping.TryMap(ref value, out finalValue)) continue;

                pixelOut.R = overlayPixel.R;
                pixelOut.G = overlayPixel.G;
                pixelOut.B = overlayPixel.B;

                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public byte Threshold = 127;
            public ISampler<Rgba32> SamplerOpacity;
            public ISampler<TPixel> SamplerOverlay;
        }
    }
}
