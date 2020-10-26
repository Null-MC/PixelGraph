using McPbrPipeline.Internal.PixelOperations;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace McPbrPipeline.Internal.ImageProcessors
{
    internal class OverlayProcessor : PixelComposeProcessor
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var pixelIn = options.Source[context.X, context.Y];

            if (options.RedSource != ColorChannel.None)
                GetValue(in pixelIn, in options.RedSource, options.RedAction, out pixelOut.R);

            if (options.GreenSource != ColorChannel.None)
                GetValue(in pixelIn, in options.GreenSource, options.GreenAction, out pixelOut.G);

            if (options.BlueSource != ColorChannel.None)
                GetValue(in pixelIn, in options.BlueSource, options.BlueAction, out pixelOut.B);

            if (options.AlphaSource != ColorChannel.None)
                GetValue(in pixelIn, in options.AlphaSource, options.AlphaAction, out pixelOut.A);
        }

        private static void GetValue(in Rgba32 sourcePixel, in ColorChannel color, ChannelAction action, out byte value)
        {
            sourcePixel.GetChannelValue(in color, out value);
            action?.Invoke(ref value);
        }

        public class Options
        {
            public Image<Rgba32> Source {get; set;}
            public ColorChannel RedSource;
            public ChannelAction RedAction;
            public ColorChannel GreenSource;
            public ChannelAction GreenAction;
            public ColorChannel BlueSource;
            public ChannelAction BlueAction;
            public ColorChannel AlphaSource;
            public ChannelAction AlphaAction;


            public void Set(ColorChannel source, ColorChannel destination, ChannelAction action = null)
            {
                switch (destination) {
                    case ColorChannel.Red:
                        RedSource = source;
                        RedAction = action;
                        break;
                    case ColorChannel.Green:
                        GreenSource = source;
                        GreenAction = action;
                        break;
                    case ColorChannel.Blue:
                        BlueSource = source;
                        BlueAction = action;
                        break;
                    case ColorChannel.Alpha:
                        AlphaSource = source;
                        AlphaAction = action;
                        break;
                }
            }
        }
    }
}
