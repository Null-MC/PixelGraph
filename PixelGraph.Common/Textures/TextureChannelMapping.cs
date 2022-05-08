using PixelGraph.Common.ResourcePack;
using System;

namespace PixelGraph.Common.Textures
{
    internal class TextureChannelMapping
    {
        public ColorChannel InputColor;
        public float? InputValue;
        public float InputMinValue;
        public float InputMaxValue;
        public byte InputRangeMin;
        public byte InputRangeMax;
        public int InputChannelShift;
        public float InputChannelPower;
        public bool InputChannelInverted;
        //public bool InputPerceptual;
        public float InputValueScale = 1f;
        public float InputValueShift;
        //public byte? InputValueDefault;

        public bool InputEnableClipping;

        public string OutputChannelID;
        public ColorChannel OutputColor;
        //public string OutputSampler;
        public float OutputMinValue;
        public float OutputMaxValue;
        public byte OutputRangeMin;
        public byte OutputRangeMax;
        public int OutputChannelShift;
        public float OutputChannelPower;
        public bool OutputChannelInverted;
        //public bool OutputPerceptual;
        public float OutputValueScale = 1f;
        public float OutputValueShift;
        public float? OutputValueDefault;
        public float? OutputClipValue;

        public bool OutputEnableClipping;
        public bool OutputApplyOcclusion;

        public bool Convert_MetalToHcm, Convert_HcmToMetal;
        //public bool Convert_SpecularToSmooth, Convert_SmoothToSpecular;
        //public bool Convert_SpecularToRough, Convert_RoughToSpecular;

        public string SourceTag;
        public string SourceFilename;
        public int Priority;
        public string Sampler;
        //public float ValueShift;
        //public float ValueScale;
        //public bool IsMetalToF0;
        //public bool IsF0ToMetal;


        public void ApplyInputChannel(PackEncodingChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            InputColor = channel.Color ?? ColorChannel.None;
            InputMinValue = (float?)channel.MinValue ?? 0f;
            InputMaxValue = (float?)channel.MaxValue ?? 1f;
            InputRangeMin = channel.RangeMin ?? 0;
            InputRangeMax = channel.RangeMax ?? 255;
            InputChannelShift = channel.Shift ?? 0;
            InputChannelPower = (float?)channel.Power ?? 1f;
            InputChannelInverted = channel.Invert ?? false;
            //InputPerceptual = channel.Perceptual ?? false;
            InputEnableClipping = channel.EnableClipping ?? false;

            Sampler = channel.Sampler;
        }

        public void ApplyOutputChannel(PackEncodingChannel channel)
        {
            OutputChannelID = channel.ID;
            OutputColor = channel.Color ?? ColorChannel.None;
            OutputMinValue = (float?)channel.MinValue ?? 0f;
            OutputMaxValue = (float?)channel.MaxValue ?? 1f;
            OutputRangeMin = channel.RangeMin ?? 0;
            OutputRangeMax = channel.RangeMax ?? 255;
            OutputChannelShift = channel.Shift ?? 0;
            OutputChannelPower = (float?)channel.Power ?? 1f;
            OutputChannelInverted = channel.Invert ?? false;
            //OutputPerceptual = channel.Perceptual ?? false;
            //OutputApplyOcclusion = channel.ApplyOcclusion ?? false;
            OutputEnableClipping = channel.EnableClipping ?? false;

            OutputValueDefault = (float?)channel.DefaultValue;
            OutputClipValue = (float?)channel.ClipValue;
            Priority = channel.Priority ?? 0;

            Sampler = channel.Sampler;
        }
    }
}
