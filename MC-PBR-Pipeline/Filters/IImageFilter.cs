using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal interface IImageFilter
    {
        void Apply(IImageProcessingContext context);
    }
}
