using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace PixelGraph.Common.PixelOperations
{
    internal abstract class PixelProcessor : IImageProcessor
    {
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, in sourceRectangle, ProcessPixel);
        }

        protected virtual void ProcessPixel<TPixel>(ref TPixel pixel, in PixelContext context)
            where TPixel : unmanaged, IPixel<TPixel> {}


        private class Processor<TPixel> : ImageProcessor<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly PixelAction<TPixel> action;


            public Processor(Configuration configuration, Image<TPixel> source, in Rectangle sourceRectangle, PixelAction<TPixel> action)
                : base(configuration, source, sourceRectangle)
            {
                this.action = action;
            }

            protected override void OnFrameApply(ImageFrame<TPixel> source)
            {
                var operation = new FilterRowOperation(source, action, SourceRectangle);
                ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
            }

            private readonly struct FilterRowOperation : IRowOperation
            {
                private readonly ImageFrame<TPixel> frame;
                private readonly PixelAction<TPixel> action;
                private readonly Rectangle region;


                public FilterRowOperation(ImageFrame<TPixel> frame, PixelAction<TPixel> action, Rectangle region)
                {
                    this.frame = frame;
                    this.action = action;
                    this.region = region;
                }

                public void Invoke(int y)
                {
                    var context = new PixelContext(region, y);
                    var row = frame.DangerousGetPixelRowMemory(y).Span;

                    for (var x = region.Left; x < region.Right; x++) {
                        context.X = x;
                        action.Invoke(ref row[x], in context);
                    }
                }
            }
        }
    }
}
