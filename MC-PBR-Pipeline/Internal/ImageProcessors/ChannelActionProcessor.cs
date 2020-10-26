using System.Collections.Generic;
using McPbrPipeline.Internal.PixelOperations;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp.PixelFormats;

namespace McPbrPipeline.Internal.ImageProcessors
{
    internal class ChannelActionProcessor : PixelFilterProcessor
    {
        public ChannelOptions Options {get;}


        public ChannelActionProcessor(ChannelOptions options = null)
        {
            Options = options ?? new ChannelOptions();
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {
            var redActionCount = Options.RedActions?.Count ?? 0;
            var greenActionCount = Options.GreenActions?.Count ?? 0;
            var blueActionCount = Options.BlueActions?.Count ?? 0;
            var alphaActionCount = Options.AlphaActions?.Count ?? 0;

            int i;
            for (i = 0; i < redActionCount; i++)
                Options.RedActions?[i](ref pixel.R);

            for (i = 0; i < greenActionCount; i++)
                Options.GreenActions?[i](ref pixel.G);

            for (i = 0; i < blueActionCount; i++)
                Options.BlueActions?[i](ref pixel.B);

            for (i = 0; i < alphaActionCount; i++)
                Options.AlphaActions?[i](ref pixel.A);

            Options.RedPostAction?.Invoke(ref pixel.R);
            Options.GreenPostAction?.Invoke(ref pixel.G);
            Options.BluePostAction?.Invoke(ref pixel.B);
            Options.AlphaPostAction?.Invoke(ref pixel.A);
        }
    }

    internal class ChannelOptions
    {
        public List<ChannelAction> RedActions {get; set;}
        public List<ChannelAction> GreenActions {get; set;}
        public List<ChannelAction> BlueActions {get; set;}
        public List<ChannelAction> AlphaActions {get; set;}
        public ChannelAction RedPostAction {get; set;}
        public ChannelAction GreenPostAction {get; set;}
        public ChannelAction BluePostAction {get; set;}
        public ChannelAction AlphaPostAction {get; set;}


        public ChannelOptions()
        {
            RedActions = new List<ChannelAction>();
            GreenActions = new List<ChannelAction>();
            BlueActions = new List<ChannelAction>();
            AlphaActions = new List<ChannelAction>();
        }

        public ChannelOptions Append(ColorChannel color, ChannelAction action)
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

        public ChannelOptions SetPost(ColorChannel color, ChannelAction action)
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

    public delegate void ChannelAction(ref byte value);
}
