using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal class NormalMapFilter : IImageFilter
    {
        public NormalMapOptions Options {get; set;}


        public NormalMapFilter()
        {
            Options = new NormalMapOptions();
        }

        public void Apply(IImageProcessingContext context)
        {
            var processor = new NormalMapProcessor {
                Options = Options,
            };

            context.ApplyProcessor(processor);
        }
    }

    internal class NormalMapOptions
    {
        public float Strength {get; set;} = 100f;
        public bool Wrap {get; set;} = true;
    }
}
