using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace McPbrPipeline.ImageProcessors.Operations
{
    internal readonly struct ComposeRowOperation<TPixel> : IRowOperation where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ImageFrame<TPixel> frame;
        private readonly Image<Rgba32> altSource;
        private readonly PixelAction action;


        public ComposeRowOperation(
            ImageFrame<TPixel> frame,
            Image<Rgba32> altSource,
            PixelAction action)
        {
            this.frame = frame;
            this.altSource = altSource;
            this.action = action;
        }

        public void Invoke(int y)
        {
            var row = frame.GetPixelRowSpan(y);

            Span<Rgba32> altRow = null;
            if (altSource != null)
                altRow = altSource.GetPixelRowSpan(y);

            var pixel = new Rgba32();
            for (var x = 0; x < frame.Width; x++) {
                if (altSource != null)
                    altRow[x].ToRgba32(ref pixel);
                else
                    row[x].ToRgba32(ref pixel);

                action.Invoke(ref pixel);
                row[x].FromRgba32(pixel);
            }
        }
    }
}
