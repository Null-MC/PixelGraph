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
            //var scaleX = (float)options.SourceWidth / context.Bounds.Width;
            //var scaleY = (float)options.SourceHeight / context.Bounds.Height;
            //var fy = context.Y * scaleY;

            var pixelOut = new Rgba32();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                //var fx = x * scaleX;

                foreach (var channel in options.Channels) {
                    var fx = (float) x / context.Bounds.Width;
                    var fy = (float) context.Y / context.Bounds.Height;
                    channel.Sampler.Sample(fx, fy, channel.Color, out var value);

                    var channelMin = channel.RangeMin;
                    var channelMax = channel.RangeMax;
                    if (value < channelMin || value > channelMax) continue;

                    pixelOut.SetChannelValue(in channel.Color, in value);
                }

                row[x].FromRgba32(pixelOut);
            }
        }

        public class Options
        {
            public List<ChannelOptions> Channels {get; set;}
            //public int SourceWidth {get; set;}
            //public int SourceHeight {get; set;}
            //public int TargetWidth {get; set;}
            //public int TargetHeight {get; set;}


            public Options()
            {
                Channels = new List<ChannelOptions>();
            }
        }

        public class ChannelOptions
        {
            public ColorChannel Color;
            public ISampler<TPixel> Sampler;
            public byte RangeMin;
            public byte RangeMax;
        }
    }
}
