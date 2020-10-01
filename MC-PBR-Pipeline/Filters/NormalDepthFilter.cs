using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal class NormalDepthFilter : IImageFilter
    {
        public NormalMapOptions Options {get; set;}


        public NormalDepthFilter(NormalMapOptions options = null)
        {
            Options = options ?? new NormalMapOptions();
        }

        public void Apply(IImageProcessingContext context)
        {
            var processor = new NormalDepthProcessor(Options);
            context.ApplyProcessor(processor);
        }
    }
}
