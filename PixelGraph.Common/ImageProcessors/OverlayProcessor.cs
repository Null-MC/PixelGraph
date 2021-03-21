using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OverlayProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var pixelOut = new Rgba32();

            var mappingCount = options.SamplerMap.Count;
            var samplerKeys = options.SamplerMap.Keys.ToArray();

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixelOut);

                for (var i = 0; i < mappingCount; i++) {
                    var mapping = samplerKeys[i];
                    pixelOut.GetChannelValue(in mapping.OutputColor, out var existingValue);

                    if (existingValue != 0) {
                        if (existingValue < mapping.OutputRangeMin || existingValue > mapping.OutputRangeMax) continue;
                    }

                    byte pixelValue = 0;
                    if (options.SamplerMap.TryGetValue(mapping, out var sampler)) {
                        var (fx, fy) = GetTexCoord(in context, in x);
                        sampler.Sample(fx, fy, in mapping.InputColor, out pixelValue);
                    }

                    if (!mapping.TryUnmap(ref pixelValue, out var value)) continue;

                    if (!mapping.ValueShift.Equal(0f))
                        value += mapping.ValueShift;

                    if (!mapping.ValueScale.Equal(1f))
                        value *= mapping.ValueScale;

                    mapping.Map(ref value, out var finalValue);

                    if (options.IsGrayscale) {
                        pixelOut.R = finalValue;
                        pixelOut.G = finalValue;
                        pixelOut.B = finalValue;
                    }
                    else {
                        pixelOut.SetChannelValue(mapping.OutputColor, finalValue);
                    }
                }

                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public Dictionary<TextureChannelMapping, ISampler<TPixel>> SamplerMap;
            public bool IsGrayscale;
        }
    }
}
