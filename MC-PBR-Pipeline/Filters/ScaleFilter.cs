using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal class ScaleFilter : IImageFilter
    {
        public ScaleOptions Options {get; set;}


        public ScaleFilter()
        {
            Options = new ScaleOptions();
        }

        public ScaleFilter(ScaleOptions options)
        {
            Options = options;
        }

        public void Apply(IImageProcessingContext context)
        {
        }
    }
}
