using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.IO
{
    public interface IImageWriter
    {
        public string Format {get; set;}
        //bool HasColor {get; set;}
        //bool HasAlpha {get; set;}

        Task WriteAsync(Image image, string localFile, ImageChannels type, CancellationToken token);
    }

    internal class ImageWriter : IImageWriter
    {
        private static readonly Dictionary<string, Func<ImageChannels, IImageEncoder>> map;
        private readonly IOutputWriter writer;

        public string Format {get; set;}
        //public bool HasColor {get; set;}
        //public bool HasAlpha {get; set;}


        static ImageWriter()
        {
            map = new Dictionary<string, Func<ImageChannels, IImageEncoder>>(StringComparer.InvariantCultureIgnoreCase) {
                ["bmp"] = GetBitmapEncoder,
                ["png"] = GetPngEncoder,
                ["tga"] = GetTgaEncoder,
                ["jpg"] = GetJpegEncoder,
                ["jpeg"] = GetJpegEncoder,
                ["gif"] = GetGifEncoder,
            };
        }

        public ImageWriter(IOutputWriter writer)
        {
            this.writer = writer;
        }

        public async Task WriteAsync(Image image, string localFile, ImageChannels type, CancellationToken token)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var encoder = GetEncoder(type);
            await using var stream = writer.Open(localFile);
            await image.SaveAsync(stream, encoder, token);
        }

        private IImageEncoder GetEncoder(ImageChannels type)
        {
            if (map.TryGetValue(Format, out var encoderFunc)) return encoderFunc(type);
            throw new ApplicationException($"Unsupported image encoding '{Format}'!");
        }

        private static BmpEncoder GetBitmapEncoder(ImageChannels type)
        {
            var hasAlpha = type == ImageChannels.ColorAlpha;

            return new BmpEncoder {
                SupportTransparency = hasAlpha,
                BitsPerPixel = hasAlpha
                    ? BmpBitsPerPixel.Pixel32
                    : BmpBitsPerPixel.Pixel24,
            };
        }

        private static PngEncoder GetPngEncoder(ImageChannels type)
        {
            return new PngEncoder {
                FilterMethod = PngFilterMethod.Adaptive,
                CompressionLevel = PngCompressionLevel.BestCompression,
                BitDepth = PngBitDepth.Bit8,
                TransparentColorMode = type == ImageChannels.ColorAlpha
                    ? PngTransparentColorMode.Preserve
                    : PngTransparentColorMode.Clear,
                ColorType = type switch {
                    ImageChannels.ColorAlpha => PngColorType.RgbWithAlpha,
                    ImageChannels.Color => PngColorType.Rgb,
                    _ => PngColorType.Grayscale
                },
            };
        }

        private static TgaEncoder GetTgaEncoder(ImageChannels type)
        {
            return new TgaEncoder {
                Compression = TgaCompression.RunLength,
                BitsPerPixel = TgaBitsPerPixel.Pixel32,
            };
        }

        private static JpegEncoder GetJpegEncoder(ImageChannels type)
        {
            return new JpegEncoder {
                Subsample = JpegSubsample.Ratio444,
                Quality = 100,
            };
        }

        private static GifEncoder GetGifEncoder(ImageChannels type)
        {
            return new GifEncoder {
                ColorTableMode = GifColorTableMode.Global,
            };
        }
    }
}
