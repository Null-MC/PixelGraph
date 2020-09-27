using McPbrPipeline.ImageProcessors;
using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal class NormalMapFilter : IImageFilter
    {
        public void Apply(IImageProcessingContext context)
        {
            var processor = new NormalMapProcessor();
            //...

            context.ApplyProcessor(processor);
        }
    }
}
