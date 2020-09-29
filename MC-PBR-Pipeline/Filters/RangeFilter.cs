using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal class RangeFilter : IImageFilter
    {
        public RangeOptions Options {get; set;}


        public RangeFilter()
        {
            Options = new RangeOptions();
        }

        public RangeFilter(RangeOptions options)
        {
            Options = options;
        }

        public void Apply(IImageProcessingContext context)
        {
            var processor = new RangeProcessor(Options);
            context.ApplyProcessor(processor);
        }
    }
}
