using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Internal.Filtering
{
    internal interface IImageFilter
    {
        void Apply(IImageProcessingContext context);
    }
}
