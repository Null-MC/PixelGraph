using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;

namespace McPbrPipeline.Filters
{
    internal class ChannelMapFilter : IImageFilter
    {
        private readonly ChannelMapOptions options;


        public ChannelMapFilter(in ChannelMapOptions options)
        {
            this.options = options;
        }

        public void Apply(IImageProcessingContext context)
        {
            var processor = new ChannelMapProcessor(options);
            context.ApplyProcessor(processor);
        }
    }
}
