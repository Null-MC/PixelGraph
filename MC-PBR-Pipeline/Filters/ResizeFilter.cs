using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp.Processing;
using System;

namespace McPbrPipeline.Filters
{
    internal class ResizeFilter : IImageFilter
    {
        public string Sampler {get; set;}
        public int? TargetSize {get; set;}
        public float? Scale {get; set;}


        public void Apply(IImageProcessingContext context)
        {
            var (width, height) = context.GetCurrentSize();

            var resampler = KnownResamplers.Bicubic;

            if (Sampler != null && Samplers.TryParse(Sampler, out var _resampler))
                resampler = _resampler;

            if (TargetSize.HasValue) {
                if (width == TargetSize) return;

                context.Resize(TargetSize.Value, 0, resampler);
            }
            else if (Scale.HasValue) {
                var targetWidth = (int)Math.Max(width * Scale.Value, 1f);
                var targetHeight = (int)Math.Max(height * Scale.Value, 1f);

                context.Resize(targetWidth, targetHeight, resampler);
            }
        }
    }
}
