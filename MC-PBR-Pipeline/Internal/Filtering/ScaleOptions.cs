using McPbrPipeline.Internal.Textures;
using System;

namespace McPbrPipeline.Internal.Filtering
{
    internal class ScaleOptions
    {
        public float Red = 1f;
        public float Green = 1f;
        public float Blue = 1f;
        public float Alpha = 1f;


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

        public bool Any {
            get {
                if (Math.Abs(Red - 1) > float.Epsilon) return true;
                if (Math.Abs(Green - 1) > float.Epsilon) return true;
                if (Math.Abs(Blue - 1) > float.Epsilon) return true;
                if (Math.Abs(Alpha - 1) > float.Epsilon) return true;
                return false;
            }
        }
    }
}
