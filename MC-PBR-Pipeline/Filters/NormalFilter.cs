using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Collections.Generic;

namespace McPbrPipeline.Filters
{
    internal class NormalFilter : IImageFilter
    {
        public NormalMapOptions Options {get; set;}


        public NormalFilter(NormalMapOptions options = null)
        {
            Options = options ?? new NormalMapOptions();
        }

        public void Apply(IImageProcessingContext context)
        {
            var chain = new List<IImageProcessor>();
            var sourceSize = context.GetCurrentSize();
            var downSampleSize = new Size();

            if (Options.DownSample > 1) {
                downSampleSize.Width = sourceSize.Width / Options.DownSample;
                downSampleSize.Height = sourceSize.Height / Options.DownSample;

                var options = new ResizeOptions {
                    Mode = ResizeMode.Stretch,
                    Sampler = KnownResamplers.Bicubic,
                    Size = downSampleSize,
                };

                chain.Add(new ResizeProcessor(options, sourceSize));
            }

            if (Options.Blur > float.Epsilon) {
                chain.Add(new GaussianBlurProcessor(Options.Blur));
            }

            chain.Add(new NormalMapProcessor(Options));

            context.ApplyProcessors(chain.ToArray());
        }
    }
}
