using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System.Collections.Generic;

namespace McPbrPipeline.ImageProcessors
{
    internal class PixelProcessor : IImageProcessor
    {
        private readonly Options options;


        public PixelProcessor(in Options options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, sourceRectangle, options);
        }

        public class Options
        {
            public List<PixelAction> RedActions {get; set;}
            public List<PixelAction> GreenActions {get; set;}
            public List<PixelAction> BlueActions {get; set;}
            public List<PixelAction> AlphaActions {get; set;}
            public PixelAction RedPostAction {get; set;}
            public PixelAction GreenPostAction {get; set;}
            public PixelAction BluePostAction {get; set;}
            public PixelAction AlphaPostAction {get; set;}


            public Options()
            {
                RedActions = new List<PixelAction>();
                GreenActions = new List<PixelAction>();
                BlueActions = new List<PixelAction>();
                AlphaActions = new List<PixelAction>();
            }

            public Options Append(ColorChannel color, PixelAction action)
            {
                switch (color) {
                    case ColorChannel.Red:
                        RedActions.Add(action);
                        break;
                    case ColorChannel.Green:
                        GreenActions.Add(action);
                        break;
                    case ColorChannel.Blue:
                        BlueActions.Add(action);
                        break;
                    case ColorChannel.Alpha:
                        AlphaActions.Add(action);
                        break;
                }

                return this;
            }

            public Options SetPost(ColorChannel color, PixelAction action)
            {
                switch (color) {
                    case ColorChannel.Red:
                        RedPostAction = action;
                        break;
                    case ColorChannel.Green:
                        GreenPostAction = action;
                        break;
                    case ColorChannel.Blue:
                        BluePostAction = action;
                        break;
                    case ColorChannel.Alpha:
                        AlphaPostAction = action;
                        break;
                }

                return this;
            }
        }

        private class Processor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly Options options;


            public Processor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, Options options)
                : base(configuration, source, sourceRectangle)
            {
                this.options = options;
            }

            protected override void OnFrameApply(ImageFrame<TPixel> source)
            {
                var operation = new RowOperation<TPixel>(source, options);
                ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
            }
        }

        private readonly struct RowOperation<TPixel> : IRowOperation where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly ImageFrame<TPixel> frame;
            private readonly Options options;


            public RowOperation(
                ImageFrame<TPixel> frame,
                Options options)
            {
                this.frame = frame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var row = frame.GetPixelRowSpan(y);

                var pixel = new Rgba32();

                var redActionCount = options.RedActions?.Count ?? 0;
                var greenActionCount = options.GreenActions?.Count ?? 0;
                var blueActionCount = options.BlueActions?.Count ?? 0;
                var alphaActionCount = options.AlphaActions?.Count ?? 0;

                int i;
                for (var x = 0; x < frame.Width; x++) {
                    row[x].ToRgba32(ref pixel);

                    for (i = 0; i < redActionCount; i++)
                        options.RedActions?[i](ref pixel.R);

                    for (i = 0; i < greenActionCount; i++)
                        options.GreenActions?[i](ref pixel.G);

                    for (i = 0; i < blueActionCount; i++)
                        options.BlueActions?[i](ref pixel.B);

                    for (i = 0; i < alphaActionCount; i++)
                        options.AlphaActions?[i](ref pixel.A);

                    options.RedPostAction?.Invoke(ref pixel.R);
                    options.GreenPostAction?.Invoke(ref pixel.G);
                    options.BluePostAction?.Invoke(ref pixel.B);
                    options.AlphaPostAction?.Invoke(ref pixel.A);

                    row[x].FromRgba32(pixel);
                }
            }
        }
    }
}
