using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ChannelResizeProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public ChannelResizeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var scaleX = options.SourceWidth / context.Bounds.Width;
            var scaleY = options.SourceHeight / context.Bounds.Height;
            var fy = context.Y * scaleY;

            var pixelOut = new Rgba32();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var fx = x * scaleX;

                byte value = 0;
                foreach (var channel in options.Channels) {
                    channel.Sampler.Sample(fx, fy, channel.Color, ref value);

                    if (channel.HasRange) {
                        var channelMin = channel.MinValue ?? 0;
                        var channelMax = channel.MaxValue ?? 255;
                        if (value < channelMin || value > channelMax) continue;
                    }

                    pixelOut.SetChannelValue(in channel.Color, in value);
                }

                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public List<ChannelOptions> Channels {get; set;}
            public int SourceWidth {get; set;}
            public int SourceHeight {get; set;}
            public int TargetWidth {get; set;}
            public int TargetHeight {get; set;}


            public Options()
            {
                Channels = new List<ChannelOptions>();
            }
        }

        public class ChannelOptions
        {
            public ColorChannel Color;
            public ISampler<TPixel> Sampler;
            public byte? MinValue;
            public byte? MaxValue;

            public bool HasRange => MinValue.HasValue && MaxValue.HasValue;
        }
    }
}
