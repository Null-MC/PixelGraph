using McPbrPipeline.Internal.Textures;

namespace McPbrPipeline.Internal.Filtering
{
    internal class ChannelMapOptions
    {
        public ColorChannel RedSource {get; set;}
        public ColorChannel GreenSource {get; set;}
        public ColorChannel BlueSource {get; set;}
        public ColorChannel AlphaSource {get; set;}


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
