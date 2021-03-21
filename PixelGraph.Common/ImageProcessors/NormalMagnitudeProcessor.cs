using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalMagnitudeProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public NormalMagnitudeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TP>(in PixelRowContext context, Span<TP> row)
        {
            var magRow = options.MagSource.GetPixelRowSpan(context.Y);
            var pixelMag = new Rgba32();

            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                var normal = row[x].ToScaledVector4();
                magRow[x].ToRgba32(ref pixelMag);

                pixelMag.GetChannelValue(in options.Mapping.InputColor, out var pixelValue);
                if (!options.Mapping.TryUnmap(ref pixelValue, out var value)) continue;
                options.Mapping.Map(ref value, out pixelValue);

                normal *= pixelValue;
                row[x].FromScaledVector4(normal);
            }
        }

        public class Options
        {
            public Image<TPixel> MagSource;
            public TextureChannelMapping Mapping;
            public float Scale;

            //public ColorChannel InputChannel;
            //public float InputMinValue;
            //public float InputMaxValue;
            //public byte InputRangeMin;
            //public byte InputRangeMax;
            //public int InputShift;
            //public float InputPower;
            //public bool InputInvert;

            //public float OutputMinValue;
            //public float OutputMaxValue;
            //public byte OutputRangeMin;
            //public byte OutputRangeMax;
            //public int OutputShift;
            //public float OutputPower;
            //public bool OutputInvert;


            //public void ApplyInputChannel(ResourcePackChannelProperties channel)
            //{
            //    InputChannel = channel.Color ?? ColorChannel.None;
            //    InputMinValue = (float?)channel.MinValue ?? 0f;
            //    InputMaxValue = (float?)channel.MaxValue ?? 1f;
            //    InputRangeMin = channel.RangeMin ?? 0;
            //    InputRangeMax = channel.RangeMax ?? 255;
            //    InputShift = channel.Shift ?? 0;
            //    InputPower = (float?) channel.Power ?? 1f;
            //    InputInvert = channel.Invert ?? false;
            //}

            //public void ApplyOutputChannel(ResourcePackChannelProperties channel)
            //{
            //    OutputMinValue = (float?)channel.MinValue ?? 0f;
            //    OutputMaxValue = (float?)channel.MaxValue ?? 1f;
            //    OutputRangeMin = channel.RangeMin ?? 0;
            //    OutputRangeMax = channel.RangeMax ?? 255;
            //    OutputShift = channel.Shift ?? 0;
            //    OutputPower = (float?) channel.Power ?? 1f;
            //    OutputInvert = channel.Invert ?? false;
            //}
        }
    }
}
