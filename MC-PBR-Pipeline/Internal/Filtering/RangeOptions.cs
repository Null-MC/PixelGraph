using McPbrPipeline.Internal.Textures;

namespace McPbrPipeline.Internal.Filtering
{
    internal class RangeOptions
    {
        public float? RedMin;
        public float? RedMax;
        public float? GreenMin;
        public float? GreenMax;
        public float? BlueMin;
        public float? BlueMax;
        public float? AlphaMin;
        public float? AlphaMax;


        public void SetMin(ColorChannel channel, float value)
        {
            switch (channel) {
                case ColorChannel.Red:
                    RedMin = value;
                    break;
                case ColorChannel.Green:
                    GreenMin = value;
                    break;
                case ColorChannel.Blue:
                    BlueMin = value;
                    break;
                case ColorChannel.Alpha:
                    AlphaMin = value;
                    break;
            }
        }

        public void SetMax(ColorChannel channel, float value)
        {
            switch (channel) {
                case ColorChannel.Red:
                    RedMax = value;
                    break;
                case ColorChannel.Green:
                    GreenMax = value;
                    break;
                case ColorChannel.Blue:
                    BlueMax = value;
                    break;
                case ColorChannel.Alpha:
                    AlphaMax = value;
                    break;
            }
        }
    }
}
