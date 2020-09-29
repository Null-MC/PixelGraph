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
    internal class NormalMapFilter : IImageFilter
    {
        public NormalMapOptions Options {get; set;}


        public NormalMapFilter(NormalMapOptions options = null)
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

            chain.Add(new NormalMapProcessor {
                Options = Options,
            });

            //if (Options.DownSample > 0) {
            //    var options = new ResizeOptions {
            //        Mode = ResizeMode.Stretch,
            //        Sampler = KnownResamplers.Bicubic,
            //        Size = sourceSize,
            //    };

            //    chain.Add(new ResizeProcessor(options, downSampleSize));
            //}

            context.ApplyProcessors(chain.ToArray());
        }
    }

    internal class NormalMapOptions
    {
        public int DownSample {get; set;} = 1;
        public float Strength {get; set;} = 1f;
        public float Blur {get; set;} = 0f;
        public bool Wrap {get; set;} = true;
    }
}
