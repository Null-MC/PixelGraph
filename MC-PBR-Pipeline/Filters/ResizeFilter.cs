using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace McPbrPipeline.Filters
{
    internal class ResizeFilter : IImageFilter
    {
        public IResampler Resampler {get; set;} = KnownResamplers.Bicubic;
        public int TargetWidth {get; set;}
        public int TargetHeight {get; set;}


        public void Apply(IImageProcessingContext context)
        {
            var size = context.GetCurrentSize();

            if (size.IsEmpty || (size.Width == TargetWidth && size.Height == TargetHeight)) {
                // image is empty or already target size
                return;
            }

            context.Resize(TargetWidth, TargetHeight, Resampler);
        }
    }
}
