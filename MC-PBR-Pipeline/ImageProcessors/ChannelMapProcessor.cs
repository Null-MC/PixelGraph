using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace McPbrPipeline.ImageProcessors
{
    internal class ChannelMapProcessor : IImageProcessor
    {
        private readonly ChannelMapOptions options;


        public ChannelMapProcessor(in ChannelMapOptions options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new ChannelMapProcessor<TPixel>(configuration, source, sourceRectangle, in options);
        }
    }

    internal class ChannelMapProcessor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ChannelMapOptions options;


        public ChannelMapProcessor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, in ChannelMapOptions options)
            : base(configuration, source, sourceRectangle)
        {
            this.options = options;
        }

        protected override void OnFrameApply(ImageFrame<TPixel> source)
        {
            var operation = new RowOperation(source, options);
            ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
        }

        private readonly struct RowOperation : IRowOperation
        {
            private readonly ImageFrame<TPixel> sourceFrame;
            private readonly ChannelMapOptions options;


            public RowOperation(
                ImageFrame<TPixel> sourceFrame,
                in ChannelMapOptions options)
            {
                this.sourceFrame = sourceFrame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var row = sourceFrame.GetPixelRowSpan(y);

                var pixelIn = new Rgba32();
                var pixelOut = new Rgba32();

                for (var x = 0; x < sourceFrame.Width; x++) {
                    row[x].ToRgba32(ref pixelIn);

                    pixelOut.R = GetChannel(options.RedSource, in pixelIn);
                    pixelOut.G = GetChannel(options.GreenSource, in pixelIn);
                    pixelOut.B = GetChannel(options.BlueSource, in pixelIn);
                    pixelOut.A = GetChannel(options.AlphaSource, in pixelIn);

                    row[x].FromRgba32(pixelOut);
                }
            }

            private static byte GetChannel(in ColorChannel channel, in Rgba32 pixel, byte defaultValue = 0)
            {
                return channel switch {
                    ColorChannel.Red => pixel.R,
                    ColorChannel.Green => pixel.G,
                    ColorChannel.Blue => pixel.B,
                    ColorChannel.Alpha => pixel.A,
                    _ => defaultValue,
                };
            }
        }
    }
}
