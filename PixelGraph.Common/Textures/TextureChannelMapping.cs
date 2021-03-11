using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Textures
{
    internal class TextureChannelMapping
    {
        public ColorChannel InputColor;
        public decimal? InputValue;
        public double InputMinValue;
        public double InputMaxValue;
        public byte InputRangeMin;
        public byte InputRangeMax;
        public int InputShift;
        public double InputPower;
        public bool InputInverted;

        public ColorChannel OutputColor;
        public string OutputSampler;
        public double OutputMinValue;
        public double OutputMaxValue;
        public byte OutputRangeMin;
        public byte OutputRangeMax;
        public int OutputShift;
        public double OutputPower;
        public bool OutputInverted;
        public bool OutputApplyOcclusion;

        public string SourceTag;
        public string SourceFilename;
        public float ValueShift;
        public float ValueScale;
        //public bool IsMetalToF0;
        //public bool IsF0ToMetal;


        public void ApplyInputChannel(ResourcePackChannelProperties channel)
        {
            InputColor = channel.Color ?? ColorChannel.None;
            InputMinValue = (double?)channel.MinValue ?? 0d;
            InputMaxValue = (double?)channel.MaxValue ?? 1d;
            InputRangeMin = channel.RangeMin ?? 0;
            InputRangeMax = channel.RangeMax ?? 255;
            InputShift = channel.Shift ?? 0;
            InputPower = (double?)channel.Power ?? 1d;
            InputInverted = channel.Invert ?? false;
        }
    }
}
