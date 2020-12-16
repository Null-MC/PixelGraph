using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using PixelGraph.Common.Extensions;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ChannelResizeProcessor : PixelProcessor
    {
        private readonly Options options;


        public ChannelResizeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var fx = context.X / (float)context.Width * options.SourceWidth;
            var fy = context.Y / (float)context.Height * options.SourceHeight;

            foreach (var channel in options.Channels) {
                channel.Sampler.Sample(fx, fy, channel.Color, out var value);

                if (channel.HasRange) {
                    var channelMin = channel.MinValue ?? 0;
                    var channelMax = channel.MaxValue ?? 255;
                    if (value < channelMin || value > channelMax) continue;
                }

                pixelOut.SetChannelValue(in channel.Color, in value);
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
            public ISampler Sampler;
            public byte? MinValue;
            public byte? MaxValue;


            public bool HasRange => MinValue.HasValue && MaxValue.HasValue;
        }
    }
}
