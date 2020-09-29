using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace McPbrPipeline.Filters
{
    internal class ResizeFilter : IImageFilter
    {
        public IResampler Resampler {get; set;} = KnownResamplers.Bicubic;
        public int TargetSize {get; set;}


        public void Apply(IImageProcessingContext context)
        {
            var size = context.GetCurrentSize();
            if (size.Width == TargetSize) return;

            context.Resize(TargetSize, 0, Resampler);
        }
    }
}
