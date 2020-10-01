using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace McPbrPipeline.ImageProcessors
{
    internal class NormalDepthProcessor : IImageProcessor
    {
        private readonly NormalMapOptions options;


        public NormalDepthProcessor(NormalMapOptions options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new NormalDepthProcessor<TPixel>(configuration, source, sourceRectangle, options);
        }
    }

    internal class NormalDepthProcessor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly NormalMapOptions options;


        public NormalDepthProcessor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, NormalMapOptions options)
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
            private readonly NormalMapOptions options;


            public RowOperation(
                ImageFrame<TPixel> sourceFrame,
                in NormalMapOptions options)
            {
                this.sourceFrame = sourceFrame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var row = sourceFrame.GetPixelRowSpan(y);

                var pixel = new Rgba32();
                for (var x = 0; x < sourceFrame.Width; x++) {
                    row[x].ToRgba32(ref pixel);

                    pixel.A = MathEx.Saturate(1f - (1f - pixel.A / 255f) * options.DepthScale);

                    row[x].FromRgba32(pixel);
                }
            }
        }
    }
}
