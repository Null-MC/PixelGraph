using PixelGraph.Common.Extensions;
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
        public int InputShift;
        public float InputPower;
        public bool InputInverted;
        public float InputScale = 1f;

        public ColorChannel OutputColor;
        public string OutputSampler;
        public float OutputMinValue;
        public float OutputMaxValue;
        public byte OutputRangeMin;
        public byte OutputRangeMax;
        public int OutputShift;
        public float OutputPower;
        public bool OutputInverted;
        public bool OutputApplyOcclusion;
        public float OutputScale = 1f;

        public string SourceTag;
        public string SourceFilename;
        public float ValueShift;
        //public float ValueScale;
        //public bool IsMetalToF0;
        //public bool IsF0ToMetal;
        public bool Invert;


        public void ApplyInputChannel(ResourcePackChannelProperties channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            InputColor = channel.Color ?? ColorChannel.None;
            InputMinValue = (float?)channel.MinValue ?? 0f;
            InputMaxValue = (float?)channel.MaxValue ?? 1f;
            InputRangeMin = channel.RangeMin ?? 0;
            InputRangeMax = channel.RangeMax ?? 255;
            InputShift = channel.Shift ?? 0;
            InputPower = (float?)channel.Power ?? 1f;
            InputInverted = channel.Invert ?? false;
        }

        public void ApplyOutputChannel(ResourcePackChannelProperties channel)
        {
            OutputMinValue = (float?)channel.MinValue ?? 0f;
            OutputMaxValue = (float?)channel.MaxValue ?? 1f;
            OutputRangeMin = channel.RangeMin ?? 0;
            OutputRangeMax = channel.RangeMax ?? 255;
            OutputShift = channel.Shift ?? 0;
            OutputPower = (float?)channel.Power ?? 1f;
            OutputInverted = channel.Invert ?? false;
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
            if (InputShift != 0) MathEx.Cycle(ref valueIn, -InputShift, in InputRangeMin, in InputRangeMax);

            value = valueIn - InputRangeMin;

            // Convert from pixel-space to value-space
            var pixelInputRange = InputRangeMax - InputRangeMin;

            if (pixelInputRange > 0) {
                var valueInputRange = InputMaxValue - InputMinValue;
                if (!valueInputRange.Equal(pixelInputRange))
                    value *= valueInputRange / pixelInputRange;
            }

            value += InputMinValue;

            if (InputInverted) MathEx.Invert(ref value, InputMinValue, InputMaxValue);

            if (!InputPower.Equal(1f))
                value = MathF.Pow(value, 1f / InputPower);

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
            if (InputShift != 0) MathEx.Cycle(ref value, -InputShift, in InputRangeMin, in InputRangeMax);

            value -= InputRangeMin / 255f;

            // Convert from pixel-space to value-space
            var pixelInputRange = (InputRangeMax - InputRangeMin) / 255f;

            if (pixelInputRange > 0) {
                var valueInputRange = InputMaxValue - InputMinValue;
                if (!valueInputRange.Equal(pixelInputRange))
                    value *= valueInputRange / pixelInputRange;
            }

            value += InputMinValue;

            if (InputInverted) MathEx.Invert(ref value, InputMinValue, InputMaxValue);

            if (!InputPower.Equal(1f))
                value = MathF.Pow(value, 1f / InputPower);

            return true;
        }

        public void Map(ref float value, out byte finalValue)
        {
            if (!OutputPower.Equal(1f))
                value = MathF.Pow(value, OutputPower);

            if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            var valueRange = OutputMaxValue - OutputMinValue;
            var pixelRange = OutputRangeMax - OutputRangeMin;

            var valueOut = value - OutputMinValue;

            if (pixelRange == 0 || valueRange == 0)
                valueOut = 0f;
            else if (!valueRange.Equal(pixelRange))
                valueOut *= pixelRange / valueRange;

            valueOut += OutputRangeMin;

            finalValue = MathEx.ClampRound(valueOut, OutputRangeMin, OutputRangeMax);

            if (OutputShift != 0)
                MathEx.Cycle(ref finalValue, in OutputShift, in OutputRangeMin, in OutputRangeMax);
        }

        public void Map(ref float value, out float finalValue)
        {
            if (!OutputPower.Equal(1f))
                value = MathF.Pow(value, OutputPower);

            if (OutputInverted) MathEx.Invert(ref value, OutputMinValue, OutputMaxValue);

            // convert from value-space to pixel-space
            var valueRange = OutputMaxValue - OutputMinValue;
            var pixelRange = (OutputRangeMax - OutputRangeMin) / 255f;

            finalValue = value - OutputMinValue;

            if (pixelRange == 0 || valueRange == 0)
                finalValue = 0f;
            else if (!valueRange.Equal(pixelRange))
                finalValue *= pixelRange / valueRange;

            finalValue += OutputRangeMin / 255f;

            var rangeMin = OutputRangeMin / 255f;
            var rangeMax = OutputRangeMax / 255f;
            MathEx.Clamp(ref finalValue, rangeMin, rangeMax);

            if (OutputShift != 0)
                MathEx.Cycle(ref finalValue, in OutputShift, in OutputRangeMin, in OutputRangeMax);
        }
    }
}
