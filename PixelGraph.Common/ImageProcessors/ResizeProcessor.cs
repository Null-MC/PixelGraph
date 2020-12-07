using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ResizeProcessor : PixelProcessor
    {
        private readonly Options options;


        public ResizeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var fx = context.X / (float)options.Source.Width * context.Width;
            var fy = context.Y / (float)options.Source.Height * context.Height;

            foreach (var channel in options.Channels) {
                var color = channel.Color ?? ColorChannel.None;
                if (color == ColorChannel.None) continue;

                Sample(fx, fy, color, out var value);

                var channelMin = channel.MinValue ?? 0;
                var channelMax = channel.MaxValue ?? 255;

                if (value > channelMin && value <= channelMax)
                    pixelOut.SetChannelValue(in color, in value);
            }
        }

        private void Sample(in float fx, in float fy, in ColorChannel color, out byte value)
        {
            // TODO: Add more than just nearest filtering

            // force nearest
            var px = (int) (fx + 0.5f);
            var py = (int) (fy + 0.5f);
            var pixel = options.Source[px, py];

            pixel.GetChannelValue(in color, out value);
        }

        public class Options
        {
            public Image<Rgba32> Source {get; set;}
            public int TargetWidth {get; set;}
            public int TargetHeight {get; set;}
            public ResourcePackChannelProperties[] Channels {get; set;}
        }
    }
}
