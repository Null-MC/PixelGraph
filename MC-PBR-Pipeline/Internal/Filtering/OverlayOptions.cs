using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace McPbrPipeline.Internal.Filtering
{
    internal class OverlayOptions
    {
        public Image<Rgba32> Source {get; set;}
        public ColorChannel RedSource {get; set;}
        public ColorChannel GreenSource {get; set;}
        public ColorChannel BlueSource {get; set;}
        public ColorChannel AlphaSource {get; set;}
        public short RedPower = 0;
        public short GreenPower = 0;
        public short BluePower = 0;
        public short AlphaPower = 0;


        public void Set(ColorChannel source, ColorChannel destination)
        {
            switch (destination) {
                case ColorChannel.Red:
                    RedSource = source;
                    break;
                case ColorChannel.Green:
                    GreenSource = source;
                    break;
                case ColorChannel.Blue:
                    BlueSource = source;
                    break;
                case ColorChannel.Alpha:
                    AlphaSource = source;
                    break;
            }
        }
    }
}
