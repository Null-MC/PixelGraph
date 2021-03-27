using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalMagnitudeReadProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public NormalMagnitudeReadProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var normalRow = options.NormalTexture.GetPixelRowSpan(context.Y);

            float value;
            var pixelMag = new Rgba32();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var normalPixel = normalRow[x].ToScaledVector4();
                var magnitude = normalPixel.Length3();

                if (!options.Mapping.TryUnmap(in magnitude, out value)) continue;
                //options.Mapping.Map(ref value, out pixelValue);

                pixelMag.SetChannelValueScaled(ColorChannel.Red, in value);
                pixelMag.SetChannelValueScaled(ColorChannel.Green, in value);
                pixelMag.SetChannelValueScaled(ColorChannel.Blue, in value);
                row[x].FromRgba32(pixelMag);
            }
        }

        public class Options
        {
            public Image<TPixel> NormalTexture;
            public TextureChannelMapping Mapping;
        }
    }
}
