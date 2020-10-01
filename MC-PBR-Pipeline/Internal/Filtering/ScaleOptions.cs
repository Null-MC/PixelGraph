using McPbrPipeline.Internal.Textures;

namespace McPbrPipeline.Internal.Filtering
{
    internal struct ScaleOptions
    {
        public float? Red;
        public float? Green;
        public float? Blue;
        public float? Alpha;


        public ScaleOptions(float red = 1f, float green = 1f, float blue = 1f, float alpha = 1f)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public void Set(ColorChannel channel, float value)
        {
            switch (channel) {
                case ColorChannel.Red:
                    Red = value;
                    break;
                case ColorChannel.Green:
                    Green = value;
                    break;
                case ColorChannel.Blue:
                    Blue = value;
                    break;
                case ColorChannel.Alpha:
                    Alpha = value;
                    break;
            }
        }
    }
}
