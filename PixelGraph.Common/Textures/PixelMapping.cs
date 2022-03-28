using PixelGraph.Common.Extensions;
using System;

namespace PixelGraph.Common.Textures
{
    internal struct PixelMapping
    {
        private readonly bool hasInputChannelShift;
        private readonly int pixelInputRange;
        private readonly float valueInputRange;
        private readonly bool hasInputChannelPower;
        private readonly float inputPowerInv;
        //private readonly float inputValueScaleInv;

        private readonly bool hasOutputShift;
        private readonly int pixelOutputRange;
        private readonly float valueOutputRange;
        private readonly bool hasOutputPower;

        //public readonly ColorChannel InputColor;
        public readonly float? InputValue;
        public readonly float InputMinValue;
        public readonly float InputMaxValue;
        public readonly byte InputRangeMin;
        public readonly byte InputRangeMax;
        public readonly int InputChannelShift;
        public readonly float InputChannelPower;
        public readonly bool InputChannelInverted;
        public readonly float InputValueScale;
        public readonly float InputValueShift;

        public readonly bool InputEnableClipping;

        //public readonly ColorChannel OutputColor;
        //public readonly string OutputSampler;
        public readonly float OutputMinValue;
        public readonly float OutputMaxValue;
        public readonly byte OutputRangeMin;
        public readonly byte OutputRangeMax;
        public readonly int OutputChannelShift;
        public readonly float OutputChannelPower;
        public readonly bool OutputInverted;
        //public readonly bool OutputApplyOcclusion;
        public readonly float OutputValueScale;
        public readonly float OutputValueShift;
        //public readonly float? OutputDefaultValue;
        public readonly float? OutputClipValue;

        public readonly bool OutputEnableClipping;

        //public readonly string Sampler;
        //public string SourceTag;
        //public string SourceFilename;
        //public float ValueShift;
        //public bool Invert;


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
            OutputInverted = mapping.OutputChannelInverted;
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
        }

        public readonly bool TryUnmap(in byte pixelValue, out float value)
        {
            if (InputValue.HasValue) {
                value = InputValue.Value;
                return true;
            }

            // Discard operation if source value outside bounds
            if (InputEnableClipping && (pixelValue < InputRangeMin || pixelValue > InputRangeMax)) {
                value = 0f;
                return false;
            }

            // Input: cycle
            var valueIn = pixelValue;
            if (hasInputChannelShift) MathEx.Cycle(ref valueIn, -InputChannelShift, in InputRangeMin, in InputRangeMax);

            value = valueIn - InputRangeMin;

            // Convert from pixel-space to value-space
            //var pixelInputRange = InputRangeMax - InputRangeMin;

            if (pixelInputRange > 0) {
                //if (!valueInputRange.Equal(pixelInputRange))
                value *= valueInputRange / pixelInputRange;
            }
            else value = 0f;

            value += InputMinValue;

            if (InputChannelInverted) MathEx.Invert(ref value, InputMinValue, InputMaxValue);

            if (hasInputChannelPower) value = MathF.Pow(value, inputPowerInv);

            value += InputValueShift;
            value *= InputValueScale;
            MathEx.Clamp(ref value, in InputMinValue, in InputMaxValue);

            return true;
        }

        public readonly bool TryUnmap(in float rawValue, out float value)
        {
            if (InputValue.HasValue) {
                value = InputValue.Value;
                return true;
            }

            var pixelValue = rawValue * 255f;

            // Discard operation if source value outside bounds
            if (InputEnableClipping && (pixelValue < InputRangeMin || pixelValue > InputRangeMax)) {
                value = 0f;
                return false;
            }

            // Input: cycle
            value = rawValue;
            if (hasInputChannelShift) MathEx.Cycle(ref value, -InputChannelShift, in InputRangeMin, in InputRangeMax);

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

            if (InputChannelInverted) MathEx.Invert(ref value, InputMinValue, InputMaxValue);

            if (hasInputChannelPower) value = MathF.Pow(value, inputPowerInv);

            value += InputValueShift;
            value *= InputValueScale;

            return true;
        }

        //public readonly void Map(ref float value, out byte finalValue)
        //{
        //    value += OutputValueShift;
        //    value *= OutputValueScale;

        //    if (hasOutputPower) value = MathF.Pow(value, OutputChannelPower);

        //    if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

        //    // convert from value-space to pixel-space
        //    //var valueRange = OutputMaxValue - OutputMinValue;
        //    //var pixelRange = OutputRangeMax - OutputRangeMin;

        //    var valueOut = value - OutputMinValue;

        //    if (pixelOutputRange == 0 || valueOutputRange == 0) {
        //        valueOut = 0f;
        //    }
        //    else if (!valueOutputRange.NearEqual(pixelOutputRange))
        //        valueOut *= pixelOutputRange / valueOutputRange;

        //    valueOut += OutputRangeMin;

        //    finalValue = MathEx.ClampRound(valueOut, OutputRangeMin, OutputRangeMax);

        //    if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputChannelShift, in OutputRangeMin, in OutputRangeMax);
        //}

        public readonly bool TryMap(ref float value, out byte finalValue)
        {
            value *= OutputValueScale;
            value += OutputValueShift;

            if (hasOutputPower) value = MathF.Pow(value, OutputChannelPower);

            if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            //var valueRange = OutputMaxValue - OutputMinValue;
            //var pixelRange = OutputRangeMax - OutputRangeMin;

            var valueOut = value - OutputMinValue;

            if (pixelOutputRange == 0 || valueOutputRange == 0) {
                //valueOut = 0f;

                // WARN: This is not working for metals
                // TODO: but i think turning it off will break VPBR opacity
                //if (!valueOut.NearEqual(0f)) {
                //    finalValue = 0;
                //    return false;
                //}

                if (valueOut > OutputMaxValue) {
                    finalValue = 0;
                    return false;
                }

                valueOut = 0f;
            }
            else if (!valueOutputRange.NearEqual(pixelOutputRange))
                valueOut *= pixelOutputRange / valueOutputRange;

            valueOut += OutputRangeMin;

            if (OutputEnableClipping) {
                if (valueOut < OutputRangeMin || valueOut > OutputRangeMax) {
                    finalValue = 0;
                    return false;
                } 
            }

            finalValue = MathEx.ClampRound(valueOut, OutputRangeMin, OutputRangeMax);

            if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputChannelShift, in OutputRangeMin, in OutputRangeMax);
            return true;
        }

        public readonly void Map(ref float value, out float finalValue)
        {
            value *= OutputValueScale;
            value += OutputValueShift;

            if (hasOutputPower) value = MathF.Pow(value, OutputChannelPower);

            if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

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

            if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputChannelShift, in OutputRangeMin, in OutputRangeMax);
        }
    }
}
