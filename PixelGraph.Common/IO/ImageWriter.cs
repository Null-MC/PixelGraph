using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public interface IImageWriter
    {
        Task<long> WriteAsync(Image image, ImageChannels type, string localFile, CancellationToken token);
        IImageEncoder GetEncoder(string ext, ImageChannels type);
    }

    internal class ImageWriter : IImageWriter
    {
        private readonly Dictionary<string, Func<ImageChannels, IImageEncoder>> map;
        private readonly ITextureGraphContext context;
        private readonly IOutputWriter writer;


        public ImageWriter(
            ITextureGraphContext context,
            IOutputWriter writer)
        {
            this.context = context;
            this.writer = writer;

            map = new Dictionary<string, Func<ImageChannels, IImageEncoder>>(StringComparer.InvariantCultureIgnoreCase) {
                ["bmp"] = GetBitmapEncoder,
                ["png"] = GetPngEncoder,
                ["tga"] = GetTgaEncoder,
                ["jpg"] = GetJpegEncoder,
                ["jpeg"] = GetJpegEncoder,
                ["gif"] = GetGifEncoder,
                ["webp"] = GetWebPEncoder,
            };
        }

        public async Task<long> WriteAsync(Image image, ImageChannels type, string localFile, CancellationToken token)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var ext = Path.GetExtension(localFile)?.TrimStart('.');

            var encoder = GetEncoder(ext, type);
            return await writer.OpenWriteAsync(localFile, async stream => {
                await image.SaveAsync(stream, encoder, token);
                await stream.FlushAsync(token);
            }, token);
        }

        public IImageEncoder GetEncoder(string ext, ImageChannels type)
        {
            if (map.TryGetValue(ext, out var encoderFunc)) return encoderFunc(type);
            throw new ApplicationException($"Unsupported image encoding '{ext}'!");
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

        private PngEncoder GetPngEncoder(ImageChannels type)
        {
            var encoder = new PngEncoder {
                FilterMethod = PngFilterMethod.Adaptive,
                ChunkFilter = PngChunkFilter.ExcludeExifChunk | PngChunkFilter.ExcludeTextChunks,
                CompressionLevel = PngCompressionLevel.Level9,
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


            if (context.EnablePalette) {
                encoder.Quantizer = new WuQuantizer(new QuantizerOptions {
                    MaxColors = context.PaletteColors,
                    //Dither = ,
                    //DitherScale = ,
                });

                encoder.ColorType = PngColorType.Palette;
                encoder.BitDepth = GetBitDepth(context.PaletteColors);
            }

            return encoder;
        }

        private PngBitDepth GetBitDepth(in int colors)
        {
            if (colors < 1 || colors > 256) throw new ArgumentOutOfRangeException(nameof(colors));

            if (colors <= 2) return PngBitDepth.Bit1;
            if (colors <= 4) return PngBitDepth.Bit2;
            if (colors <= 16) return PngBitDepth.Bit4;
            if (colors <= 256) return PngBitDepth.Bit8;
            //if (colors <= 256) return PngBitDepth.Bit16;

            throw new ApplicationException("Unexpected bit count!");
        }

        private static TgaEncoder GetTgaEncoder(ImageChannels type)
        {
            return new() {
                Compression = TgaCompression.RunLength,
                BitsPerPixel = TgaBitsPerPixel.Pixel32,
            };
        }

        private static JpegEncoder GetJpegEncoder(ImageChannels type)
        {
            return new() {
                Quality = 100,
                ColorType = type == ImageChannels.Gray
                    ? JpegColorType.Luminance
                    : JpegColorType.YCbCrRatio444,
            };
        }

        private static GifEncoder GetGifEncoder(ImageChannels type)
        {
            return new() {
                ColorTableMode = GifColorTableMode.Global,
            };
        }

        private static WebpEncoder GetWebPEncoder(ImageChannels type)
        {
            return new() {
                Method = WebpEncodingMethod.Default,
                FileFormat = WebpFileFormatType.Lossless,
                TransparentColorMode = WebpTransparentColorMode.Preserve,
                //UseAlphaCompression = true,
            };
        }
    }
}
