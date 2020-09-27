using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;

namespace McPbrPipeline.ImageProcessors
{
    internal class NormalMapProcessor : IImageProcessor
    {
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new NormalMapProcessor<TPixel>(this, source, configuration, sourceRectangle);
        }
    }

    internal class NormalMapProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly NormalMapProcessor processor;
        private readonly Configuration configuration;
        private readonly Rectangle sourceRectangle;
        private readonly Image<TPixel> source;


        public NormalMapProcessor(NormalMapProcessor processor, Image<TPixel> source, Configuration configuration, Rectangle sourceRectangle)
        {
            this.processor = processor;
            this.configuration = configuration;
            this.sourceRectangle = sourceRectangle;
            this.source = source;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Dispose() {}
    }
}
