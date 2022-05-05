using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;

namespace PixelGraph.Common.PixelOperations
{
    internal abstract class PixelRowProcessor : IImageProcessor
    {
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, in sourceRectangle, ProcessRow);
        }

        protected virtual void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> row)
            where TPixel : unmanaged, IPixel<TPixel> {}

        private static void GetTexCoordX(in Rectangle bounds, in float x, out double fx)
        {
            fx = (x - bounds.X + 0.5f) / bounds.Width;
        }

        private static void GetTexCoordY(in Rectangle bounds, in float y, out double fy)
        {
            fy = (y - bounds.Y + 0.5f) / bounds.Height;
        }

        protected static void GetTexCoordY(in PixelRowContext context, out double fy)
        {
            GetTexCoordY(in context.Bounds, context.Y, out fy);
        }

        protected static void GetTexCoord(in PixelRowContext context, in int x, out double fx, out double fy)
        {
            GetTexCoord(in context, x, context.Y, out fx, out fy);
        }

        protected static void GetTexCoord(in PixelRowContext context, in float x, in float y, out double fx, out double fy)
        {
            GetTexCoordX(in context.Bounds, in x, out fx);
            GetTexCoordY(in context.Bounds, in y, out fy);
        }

        private class Processor<TPixel> : ImageProcessor<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly PixelRowAction<TPixel> action;


            public Processor(Configuration configuration, Image<TPixel> source, in Rectangle sourceRectangle, PixelRowAction<TPixel> action)
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
                private readonly PixelRowAction<TPixel> action;
                private readonly Rectangle region;


                public FilterRowOperation(ImageFrame<TPixel> frame, PixelRowAction<TPixel> action, Rectangle region)
                {
                    this.frame = frame;
                    this.action = action;
                    this.region = region;
                }

                public void Invoke(int y)
                {
                    if (y < 0 || y >= frame.Height) return;

                    var row = frame
                        .DangerousGetPixelRowMemory(y)
                        .Slice(region.X, region.Width).Span;

                    var context = new PixelRowContext(region, y);
                    action.Invoke(in context, row);
                }
            }
        }
    }
}
