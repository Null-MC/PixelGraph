using McPbrPipeline.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;

namespace McPbrPipeline.ImageProcessors
{
    internal class RangeProcessor : IImageProcessor
    {
        public RangeOptions Options {get; set;}


        public RangeProcessor()
        {
            Options = new RangeOptions();
        }

        public RangeProcessor(RangeOptions options)
        {
            Options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new RangeProcessor<TPixel>(this, source, configuration, sourceRectangle);
        }
    }

    internal class RangeProcessor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly RangeProcessor processor;


        public RangeProcessor(RangeProcessor processor, Image<TPixel> source, Configuration configuration, Rectangle sourceRectangle)
            : base(configuration, source, sourceRectangle)
        {
            this.processor = processor;
        }

        protected override void OnFrameApply(ImageFrame<TPixel> source)
        {
            var operation = new RowOperation(source, processor.Options);
            ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
        }

        private readonly struct RowOperation : IRowOperation
        {
            //private readonly Configuration configuration;
            private readonly ImageFrame<TPixel> sourceFrame;
            private readonly RangeOptions options;


            public RowOperation(
                //Configuration configuration,
                ImageFrame<TPixel> sourceFrame,
                RangeOptions options)
            {
                //this.configuration = configuration;
                this.sourceFrame = sourceFrame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var rMin = options.RedMin ?? 0f;
                var rMax = options.RedMax ?? 1f;
                var gMin = options.GreenMin ?? 0f;
                var gMax = options.GreenMax ?? 1f;
                var bMin = options.BlueMin ?? 0f;
                var bMax = options.BlueMax ?? 1f;
                var aMin = options.AlphaMin ?? 0f;
                var aMax = options.AlphaMax ?? 1f;

                var row = sourceFrame.GetPixelRowSpan(y);

                var pixel = new Rgba32();
                for (var x = 0; x < sourceFrame.Width; x++) {
                    row[x].ToRgba32(ref pixel);

                    if (options.RedMin.HasValue || options.RedMax.HasValue)
                        pixel.R = Filter(in pixel.R, in rMin, in rMax);

                    if (options.GreenMin.HasValue || options.GreenMax.HasValue)
                        pixel.G = Filter(in pixel.G, in gMin, in gMax);

                    if (options.BlueMin.HasValue || options.BlueMax.HasValue)
                        pixel.B = Filter(in pixel.B, in bMin, in bMax);

                    if (options.AlphaMin.HasValue || options.AlphaMax.HasValue)
                        pixel.A = Filter(in pixel.A, in aMin, in aMax);

                    row[x].FromRgba32(pixel);
                }
            }

            private static byte Filter(in byte value, in float min, in float max)
            {
                var result = value / 255f * (max - min) + min;

                return Math.Clamp((byte)(result * 255f), (byte)0, (byte)255);
            }
        }
    }
}
