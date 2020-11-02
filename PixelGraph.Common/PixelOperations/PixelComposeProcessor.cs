using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace PixelGraph.Common.PixelOperations
{
    internal abstract class PixelComposeProcessor : IImageProcessor
    {
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, in sourceRectangle, ProcessPixel);
        }

        protected abstract void ProcessPixel(ref Rgba32 pixel, in PixelContext context);

        private class Processor<TPixel> : ImageProcessor<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly PixelAction action;


            public Processor(Configuration configuration, Image<TPixel> source, in Rectangle sourceRectangle, PixelAction action)
                : base(configuration, source, sourceRectangle)
            {
                this.action = action;
            }

            protected override void OnFrameApply(ImageFrame<TPixel> source)
            {
                var operation = new FilterRowOperation(source, action);
                ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
            }

            private readonly struct FilterRowOperation : IRowOperation
            {
                private readonly ImageFrame<TPixel> frame;
                private readonly PixelAction action;


                public FilterRowOperation(ImageFrame<TPixel> frame, PixelAction action)
                {
                    this.frame = frame;
                    this.action = action;
                }

                public void Invoke(int y)
                {
                    var context = new PixelContext(frame.Width, frame.Height, y);
                    var row = frame.GetPixelRowSpan(y);
                    var pixel = new Rgba32();

                    for (var x = 0; x < frame.Width; x++) {
                        context.X = x;

                        row[x].ToRgba32(ref pixel);
                        action.Invoke(ref pixel, in context);
                        row[x].FromRgba32(pixel);
                    }
                }
            }
        }
    }
}
