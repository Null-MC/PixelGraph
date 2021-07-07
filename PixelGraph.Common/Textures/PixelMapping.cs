using PixelGraph.Common.Extensions;
using System;

namespace PixelGraph.Common.Textures
{
    internal class PixelMapping
    {
        private readonly bool hasInputShift;
        private readonly int pixelInputRange;
        private readonly float valueInputRange;
        private readonly bool hasInputPower;
        private readonly float inputPowerInv;

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
        public readonly int InputShift;
        public readonly float InputPower;
        public readonly bool InputInverted;
        public readonly float InputScale;

        //public readonly ColorChannel OutputColor;
        public readonly string OutputSampler;
        public readonly float OutputMinValue;
        public readonly float OutputMaxValue;
        public readonly byte OutputRangeMin;
        public readonly byte OutputRangeMax;
        public readonly int OutputShift;
        public readonly float OutputPower;
        public readonly bool OutputInverted;
        public readonly bool OutputApplyOcclusion;
        public readonly float OutputScale;

        //public string SourceTag;
        //public string SourceFilename;
        //public float ValueShift;
        //public bool Invert;


        public PixelMapping(TextureChannelMapping mapping)
        {
            //InputColor = mapping.InputColor;
            InputValue = mapping.InputValue;
            InputMinValue = mapping.InputMinValue;
            InputMaxValue = mapping.InputMaxValue;
            InputRangeMin = mapping.InputRangeMin;
            InputRangeMax = mapping.InputRangeMax;
            InputShift = mapping.InputShift;
            InputPower = mapping.InputPower;
            InputInverted = mapping.InputInverted;
            InputScale = mapping.InputScale;

            //OutputColor = mapping.OutputColor;
            OutputSampler = mapping.OutputSampler;
            OutputMinValue = mapping.OutputMinValue;
            OutputMaxValue = mapping.OutputMaxValue;
            OutputRangeMin = mapping.OutputRangeMin;
            OutputRangeMax = mapping.OutputRangeMax;
            OutputShift = mapping.OutputShift;
            OutputPower = mapping.OutputPower;
            OutputInverted = mapping.OutputInverted;
            OutputScale = mapping.OutputScale;
            OutputApplyOcclusion = mapping.OutputApplyOcclusion;

            // TODO: copy all mapping properties

            hasInputShift = mapping.InputShift != 0;
            pixelInputRange = InputRangeMax - InputRangeMin;
            valueInputRange = InputMaxValue - InputMinValue;
            hasInputPower = !InputPower.Equal(1f);
            inputPowerInv = 1f / InputPower;

            hasOutputShift = mapping.OutputShift != 0;
            pixelOutputRange = OutputRangeMax - OutputRangeMin;
            valueOutputRange = OutputMaxValue - OutputMinValue;
            hasOutputPower = !OutputPower.Equal(1f);
        }

        public bool TryUnmap(in byte pixelValue, out float value)
        {
            if (InputValue.HasValue) {
                value = InputValue.Value;
                return true;
            }

            // Discard operation if source value outside bounds
            if (pixelValue < InputRangeMin || pixelValue > InputRangeMax) {
                value = 0f;
                return false;
            }

            // Input: cycle
            var valueIn = pixelValue;
            if (hasInputShift) MathEx.Cycle(ref valueIn, -InputShift, in InputRangeMin, in InputRangeMax);

            value = valueIn - InputRangeMin;

            // Convert from pixel-space to value-space
            //var pixelInputRange = InputRangeMax - InputRangeMin;

            if (pixelInputRange > 0) {
                //if (!valueInputRange.Equal(pixelInputRange))
                value *= valueInputRange / pixelInputRange;
            }
            else value = 0f;

            value += InputMinValue;

            if (InputInverted) MathEx.Invert(ref value, InputMinValue, InputMaxValue);

            if (hasInputPower) value = MathF.Pow(value, inputPowerInv);

            // TODO: shift & scale?!

            return true;
        }

        public bool TryUnmap(in float rawValue, out float value)
        {
            if (InputValue.HasValue) {
                value = InputValue.Value;
                return true;
            }

            var pixelValue = rawValue * 255f;

            // Discard operation if source value outside bounds
            if (pixelValue < InputRangeMin || pixelValue > InputRangeMax) {
                value = 0f;
                return false;
            }

            // Input: cycle
            value = rawValue;
            if (hasInputShift) MathEx.Cycle(ref value, -InputShift, in InputRangeMin, in InputRangeMax);

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

            if (InputInverted) MathEx.Invert(ref value, InputMinValue, InputMaxValue);

            if (hasInputPower) value = MathF.Pow(value, inputPowerInv);

            // TODO: shift & scale?!

            return true;
        }

        public void Map(ref float value, out byte finalValue)
        {
            if (hasOutputPower) value = MathF.Pow(value, OutputPower);

            if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            //var valueRange = OutputMaxValue - OutputMinValue;
            //var pixelRange = OutputRangeMax - OutputRangeMin;

            var valueOut = value - OutputMinValue;

            if (pixelOutputRange == 0 || valueOutputRange == 0)
                valueOut = 0f;
            else if (!valueOutputRange.Equal(pixelOutputRange))
                valueOut *= pixelOutputRange / valueOutputRange;

            valueOut += OutputRangeMin;

            finalValue = MathEx.ClampRound(valueOut, OutputRangeMin, OutputRangeMax);

            if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputShift, in OutputRangeMin, in OutputRangeMax);
        }

        public void Map(ref float value, out float finalValue)
        {
            if (hasOutputPower) value = MathF.Pow(value, OutputPower);

            if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            //var valueRange = OutputMaxValue - OutputMinValue;
            var _pixelRange = pixelOutputRange / 255f;

            finalValue = value - OutputMinValue;

            if (_pixelRange == 0 || valueOutputRange == 0)
                finalValue = 0f;
            else if (!valueOutputRange.Equal(_pixelRange))
                finalValue *= _pixelRange / valueOutputRange;

            finalValue += OutputRangeMin / 255f;

            var rangeMin = OutputRangeMin / 255f;
            var rangeMax = OutputRangeMax / 255f;
            MathEx.Clamp(ref finalValue, rangeMin, rangeMax);

            if (hasOutputShift) MathEx.Cycle(ref finalValue, in OutputShift, in OutputRangeMin, in OutputRangeMax);
        }
    }
}
