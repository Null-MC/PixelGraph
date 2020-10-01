using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;
using McPbrPipeline.Internal;

namespace McPbrPipeline.ImageProcessors
{
    internal class ScaleProcessor : IImageProcessor
    {
        public ScaleOptions Options {get; set;}


        public ScaleProcessor()
        {
            Options = new ScaleOptions();
        }

        public ScaleProcessor(ScaleOptions options)
        {
            Options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new ScaleProcessor<TPixel>(this, source, configuration, sourceRectangle);
        }
    }

    internal class ScaleProcessor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ScaleProcessor processor;


        public ScaleProcessor(ScaleProcessor processor, Image<TPixel> source, Configuration configuration, Rectangle sourceRectangle)
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
            private readonly ScaleOptions options;


            public RowOperation(
                //Configuration configuration,
                ImageFrame<TPixel> sourceFrame,
                in ScaleOptions options)
            {
                //this.configuration = configuration;
                this.sourceFrame = sourceFrame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var rScale = options.Red ?? 1f;
                var gScale = options.Green ?? 1f;
                var bScale = options.Blue ?? 1f;
                var aScale = options.Alpha ?? 1f;

                var row = sourceFrame.GetPixelRowSpan(y);

                var pixel = new Rgba32();
                for (var x = 0; x < sourceFrame.Width; x++) {
                    row[x].ToRgba32(ref pixel);

                    if (options.Red.HasValue)
                        pixel.R = Filter(in pixel.R, in rScale);

                    if (options.Green.HasValue)
                        pixel.G = Filter(in pixel.G, in gScale);

                    if (options.Blue.HasValue)
                        pixel.B = Filter(in pixel.B, in bScale);

                    if (options.Alpha.HasValue)
                        pixel.A = Filter(in pixel.A, in aScale);

                    row[x].FromRgba32(pixel);
                }
            }

            private static byte Filter(in byte value, in float scale)
            {
                return MathEx.Saturate(value / 255f * scale);
            }
        }
    }
}
