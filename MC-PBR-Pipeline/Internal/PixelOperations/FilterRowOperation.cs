using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace McPbrPipeline.ImageProcessors.Operations
{
    internal readonly struct FilterRowOperation<TPixel> : IRowOperation where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ImageFrame<TPixel> frame;
        private readonly PixelAction action;


        public FilterRowOperation(ImageFrame<TPixel> frame, PixelAction action)
        {
            this.frame = frame;
            this.action = action;
        }

        public void Invoke(int y)
        {
            var row = frame.GetPixelRowSpan(y);

            var pixel = new Rgba32();
            for (var x = 0; x < frame.Width; x++) {
                row[x].ToRgba32(ref pixel);
                action.Invoke(ref pixel);
                row[x].FromRgba32(pixel);
            }
        }
    }
}
