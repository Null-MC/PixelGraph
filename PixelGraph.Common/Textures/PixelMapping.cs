using PixelGraph.Common.Extensions;
using System;

namespace PixelGraph.Common.Textures
{
    internal struct PixelMapping
    {
        private bool hasInputChannelShift;
        private int pixelInputRange;
        private float valueInputRange;
        private bool hasInputChannelPower;
        private float inputPowerInv;
        //private readonly float inputValueScaleInv;

        private bool hasOutputShift;
        private int pixelOutputRange;
        private float valueOutputRange;
        private bool hasOutputPower;

        //public readonly ColorChannel InputColor;
        public float? InputValue;
        public float InputMinValue;
        public float InputMaxValue;
        public byte InputRangeMin;
        public byte InputRangeMax;
        public int InputChannelShift;
        public float InputChannelPower;
        public bool InputChannelInverted;
        public float InputValueScale;
        public float InputValueShift;

        public bool InputEnableClipping;

        //public readonly ColorChannel OutputColor;
        //public readonly string OutputSampler;
        public float OutputMinValue;
        public float OutputMaxValue;
        public byte OutputRangeMin;
        public byte OutputRangeMax;
        public int OutputChannelShift;
        public float OutputChannelPower;
        public bool OutputChannelInverted;
        //public readonly bool OutputApplyOcclusion;
        public float OutputValueScale;
        public float OutputValueShift;
        //public readonly float? OutputDefaultValue;
        public float? OutputClipValue;

        public bool OutputEnableClipping;

        //public readonly string Sampler;
        //public string SourceTag;
        //public string SourceFilename;
        //public float ValueShift;
        //public bool Invert;

        public bool Convert_MetalToHcm, Convert_HcmToMetal;
        //public bool Convert_SpecularToSmooth, Convert_SmoothToSpecular;
        //public bool Convert_SpecularToRough, Convert_RoughToSpecular;


        public PixelMapping(TextureChannelMapping mapping)
        {
            //InputColor = mapping.InputColor;
            InputValue = mapping.InputValue; // ?? mapping.InputValueDefault;
            InputMinValue = mapping.InputMinValue;
            InputMaxValue = mapping.InputMaxValue;
            InputRangeMin = mapping.InputRangeMin;
            InputRangeMax = mapping.InputRangeMax;
            InputChannelShift = mapping.InputChannelShift;
            InputChannelPower = mapping.InputChannelPower;
            InputChannelInverted = mapping.InputChannelInverted;
            //InputPerceptual = mapping.InputPerceptual;
            InputValueScale = mapping.InputValueScale;
            InputValueShift = mapping.InputValueShift;

            InputEnableClipping = mapping.InputEnableClipping;

            //OutputColor = mapping.OutputColor;
            //OutputSampler = mapping.OutputSampler;
            OutputMinValue = mapping.OutputMinValue;
            OutputMaxValue = mapping.OutputMaxValue;
            OutputRangeMin = mapping.OutputRangeMin;
            OutputRangeMax = mapping.OutputRangeMax;
            OutputChannelShift = mapping.OutputChannelShift;
            OutputChannelPower = mapping.OutputChannelPower;
            OutputChannelInverted = mapping.OutputChannelInverted;
            //OutputPerceptual = mapping.OutputPerceptual;
            OutputValueScale = mapping.OutputValueScale;
            OutputValueShift = mapping.OutputValueShift;
            //OutputApplyOcclusion = mapping.OutputApplyOcclusion;
            //OutputDefaultValue = mapping.OutputValueDefault;
            OutputClipValue = mapping.OutputClipValue;

            OutputEnableClipping = mapping.OutputEnableClipping;

            //Sampler = mapping.Sampler;

            // TODO: copy all mapping properties

            hasInputChannelShift = mapping.InputChannelShift != 0;
            pixelInputRange = InputRangeMax - InputRangeMin;
            valueInputRange = InputMaxValue - InputMinValue;
            hasInputChannelPower = !InputChannelPower.NearEqual(1f);
            inputPowerInv = 1f / InputChannelPower;
            //inputValueScaleInv = 1f / InputValueScale;

            hasOutputShift = mapping.OutputChannelShift != 0;
            pixelOutputRange = OutputRangeMax - OutputRangeMin;
            valueOutputRange = OutputMaxValue - OutputMinValue;
            hasOutputPower = !OutputChannelPower.NearEqual(1f);

            Convert_HcmToMetal = mapping.Convert_HcmToMetal;
            Convert_MetalToHcm = mapping.Convert_MetalToHcm;
            //Convert_SpecularToSmooth = mapping.Convert_SpecularToSmooth;
            //Convert_SmoothToSpecular = mapping.Convert_SmoothToSpecular;
            //Convert_SpecularToRough = mapping.Convert_SpecularToRough;
            //Convert_RoughToSpecular = mapping.Convert_RoughToSpecular;
        }

        public readonly bool TryUnmap(in float rawValue, out float value)
        {
            if (InputValue.HasValue) {
                value = InputValue.Value;
                return true;
            }

            //var pixelValue = rawValue * 255f;
            var rangeMin = InputRangeMin / 255f;
            var rangeMax = InputRangeMax / 255f;

            // Discard operation if source value outside bounds
            if (InputEnableClipping && (rawValue < rangeMin || rawValue > rangeMax)) {
                value = 0f;
                return false;
            }

            value = rawValue;

            // Input: cycle
            if (hasInputChannelShift) MathEx.Cycle(ref value, -InputChannelShift, in rangeMin, in rangeMax);

            value -= InputRangeMin / 255f;

            // Convert from pixel-space to value-space

            if (pixelInputRange > 0) {
                //var valueInputRange = InputMaxValue - InputMinValue;
                //if (!valueInputRange.Equal(pixelInputRange))
                var _pixelRange = pixelInputRange / 255f;
                value *= valueInputRange / _pixelRange;
            }
            else value = 0f;

            value += InputMinValue;

            if (InputChannelInverted) MathEx.InvertRef(ref value, InputMinValue, InputMaxValue);

            if (hasInputChannelPower) value = MathF.Pow(value, inputPowerInv);

            // WARN: Which should come first?!
            value *= InputValueScale;
            value += InputValueShift;

            return true;
        }

        public readonly bool TryMap(ref float value, out float finalValue)
        {
            // WARN: Which should come first?!
            value += OutputValueShift;
            value *= OutputValueScale;

            if (hasOutputPower) value = MathF.Pow(value, OutputChannelPower);

            if (OutputChannelInverted) MathEx.InvertRef(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            //var valueRange = OutputMaxValue - OutputMinValue;
            var _pixelRange = pixelOutputRange / 255f;

            finalValue = value - OutputMinValue;

            if (_pixelRange == 0 || valueOutputRange == 0)
                finalValue = 0f;
            else if (!valueOutputRange.NearEqual(_pixelRange))
                finalValue *= _pixelRange / valueOutputRange;

            finalValue += OutputRangeMin / 255f;

            var rangeMin = OutputRangeMin / 255f;
            var rangeMax = OutputRangeMax / 255f;

            if (OutputEnableClipping) {
                if (finalValue < rangeMin || finalValue > rangeMax) {
                    //finalValue = 0;
                    return false;
                } 
            }

            MathEx.Clamp(ref finalValue, rangeMin, rangeMax);

            if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputChannelShift, in rangeMin, in rangeMax);

            return true;
        }

        public readonly void Map(ref float value, out float finalValue)
        {
            // WARN: Which should come first?!
            value += OutputValueShift;
            value *= OutputValueScale;

            if (hasOutputPower) value = MathF.Pow(value, OutputChannelPower);

            if (OutputChannelInverted) MathEx.InvertRef(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            //var valueRange = OutputMaxValue - OutputMinValue;
            var _pixelRange = pixelOutputRange / 255f;

            finalValue = value - OutputMinValue;

            if (_pixelRange == 0 || valueOutputRange == 0)
                finalValue = 0f;
            else if (!valueOutputRange.NearEqual(_pixelRange))
                finalValue *= _pixelRange / valueOutputRange;

            finalValue += OutputRangeMin / 255f;

            var rangeMin = OutputRangeMin / 255f;
            var rangeMax = OutputRangeMax / 255f;
            MathEx.Clamp(ref finalValue, rangeMin, rangeMax);

            if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputChannelShift, in rangeMin, in rangeMax);
        }
    }
}
