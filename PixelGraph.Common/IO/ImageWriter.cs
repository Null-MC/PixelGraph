using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Text;

namespace PixelGraph.Common.IO;

public interface IImageWriter
{
    IImageEncoder GetEncoder(ImageFormats format, ImageChannels type);
    Task<long> WriteAsync(Image image, ImageChannels type, string localFile, CancellationToken token);
}

internal class ImageWriter : IImageWriter
{
    private readonly Dictionary<ImageFormats, Func<ImageChannels, IImageEncoder>> decoderMap;
    private readonly ITextureGraphContext context;
    private readonly IOutputWriter writer;


    public ImageWriter(
        ITextureGraphContext context,
        IOutputWriter writer)
    {
        this.context = context;
        this.writer = writer;

        decoderMap = new Dictionary<ImageFormats, Func<ImageChannels, IImageEncoder>> {
            [ImageFormats.Bitmap] = GetBitmapEncoder,
            [ImageFormats.Png] = GetPngEncoder,
            [ImageFormats.Tga] = GetTgaEncoder,
            [ImageFormats.Jpeg] = GetJpegEncoder,
            [ImageFormats.Gif] = GetGifEncoder,
            [ImageFormats.WebP] = GetWebPEncoder,
        };
    }

    public async Task<long> WriteAsync(Image image, ImageChannels type, string localFile, CancellationToken token)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));

        var ext = Path.GetExtension(localFile).TrimStart('.');

        if (!ImageFormat.TryParse(ext, out var format))
            throw new ApplicationException($"Unsupported image encoding '{ext}'!");

        var encoder = GetEncoder(format, type);
        PopulateMetadata(image.Metadata, format);

        return await writer.OpenWriteAsync(localFile, async stream => {
            await image.SaveAsync(stream, encoder, token);
            await stream.FlushAsync(token);
        }, token);
    }

    private void PopulateMetadata(ImageMetadata metadata, ImageFormats format)
    {
        metadata.ExifProfile = BuildExifProfile();
        metadata.IptcProfile = BuildIptcProfile();

        switch (format) {
            case ImageFormats.Png:
                var pngMeta = metadata.GetPngMetadata();
                pngMeta.TextData.Add(new PngTextData("Software", $"PixelGraph v{AppCommon.Version}", null, null));
                pngMeta.TextData.Add(new PngTextData("Creation Time", DateTime.UtcNow.ToString("R"), null, null));

                if (!string.IsNullOrWhiteSpace(context.Project.Author)) {
                    pngMeta.TextData.Add(new PngTextData("Author", context.Project.Author, null, null));
                    pngMeta.TextData.Add(new PngTextData("Copyright", $"Copyright {DateTime.Now.Year}", null, null));
                }

                break;
            case ImageFormats.Gif:
                var gifMeta = metadata.GetGifMetadata();
                gifMeta.Comments.Add($"Published using PixelGraph v{AppCommon.Version}");

                if (!string.IsNullOrWhiteSpace(context.Project.Author)) {
                    gifMeta.Comments.Add($"Author: {context.Project.Author}");
                    gifMeta.Comments.Add($"Copyright {DateTime.Now.Year}");
                }

                break;
        }
    }

    private ExifProfile BuildExifProfile()
    {
        var profile = new ExifProfile();
        profile.SetValue(ExifTag.Software, $"PixelGraph v{AppCommon.Version}");
        profile.SetValue(ExifTag.XPComment, $"Published with PixelGraph v{AppCommon.Version}");
        profile.SetValue(ExifTag.UserComment, $"Published with PixelGraph v{AppCommon.Version}");
        profile.SetValue(ExifTag.DateTime, DateTime.UtcNow.ToString("O"));

        if (!string.IsNullOrWhiteSpace(context.Project.Author)) {
            profile.SetValue(ExifTag.Artist, context.Project.Author);
            profile.SetValue(ExifTag.XPAuthor, context.Project.Author);
            profile.SetValue(ExifTag.OwnerName, context.Project.Author);
            profile.SetValue(ExifTag.Copyright, $"Copyright {DateTime.Now.Year}");
        }

        return profile;
    }

    private IptcProfile BuildIptcProfile()
    {
        var profile = new IptcProfile();
        profile.SetValue(IptcTag.OriginatingProgram, Encoding.ASCII, "PixelGraph");
        profile.SetValue(IptcTag.ProgramVersion, Encoding.ASCII, AppCommon.Version);

        if (!string.IsNullOrWhiteSpace(context.Project.Author)) {
            profile.SetValue(IptcTag.Contact, Encoding.ASCII, context.Project.Author);
            profile.SetValue(IptcTag.CopyrightNotice, Encoding.ASCII, $"Copyright {DateTime.Now.Year}");
        }

        return profile;
    }

    public static ImageFormats GetFormat(string ext)
    {
        if (ImageFormat.TryParse(ext, out var format)) return format;
        throw new ApplicationException($"Unsupported image encoding '{ext}'!");
    }

    public IImageEncoder GetEncoder(ImageFormats format, ImageChannels type)
    {
        if (decoderMap.TryGetValue(format, out var encoderFunc)) return encoderFunc(type);
        throw new ApplicationException($"No decoder found for image format '{format}'!");
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
        IQuantizer quantizer = null;
        PngColorType colorType;
        PngBitDepth bitDepth;

        if (context.EnablePalette) {
            quantizer = new WuQuantizer(new QuantizerOptions {
                MaxColors = context.PaletteColors,
            });

            colorType = PngColorType.Palette;
            bitDepth = GetBitDepth(context.PaletteColors);
        }
        else {
            bitDepth = PngBitDepth.Bit8;
            colorType = type switch {
                ImageChannels.ColorAlpha => PngColorType.RgbWithAlpha,
                ImageChannels.Color => PngColorType.Rgb,
                _ => PngColorType.Grayscale,
            };
        }

        var encoder = new PngEncoder {
            CompressionLevel = PngCompressionLevel.Level9,
            FilterMethod = PngFilterMethod.Paeth,
            ChunkFilter = PngChunkFilter.None,
            ColorType = colorType,
            Quantizer = quantizer,
            BitDepth = bitDepth,

            //TransparentColorMode = type == ImageChannels.ColorAlpha
            //    ? PngTransparentColorMode.Preserve
            //    : PngTransparentColorMode.Clear,
            TransparentColorMode = PngTransparentColorMode.Preserve,

        };

        //if (!PopulateExif) encoder.ChunkFilter |= PngChunkFilter.ExcludeExifChunk;

        return encoder;
    }

    private static PngBitDepth GetBitDepth(in int colors)
    {
        return colors switch {
            < 1 or > 65_536 => throw new ArgumentOutOfRangeException(nameof(colors), colors, "Number of palette colors must be between 1 - 65,536"),
            <= 2 => PngBitDepth.Bit1,
            <= 4 => PngBitDepth.Bit2,
            <= 16 => PngBitDepth.Bit4,
            <= 256 => PngBitDepth.Bit8,
            _ => PngBitDepth.Bit16
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
            Quality = 100,
            ColorType = type == ImageChannels.Gray
                ? JpegEncodingColor.Luminance
                : JpegEncodingColor.YCbCrRatio444,
        };
    }

    private static GifEncoder GetGifEncoder(ImageChannels type)
    {
        return new GifEncoder {
            ColorTableMode = GifColorTableMode.Global,
        };
    }

    private static WebpEncoder GetWebPEncoder(ImageChannels type)
    {
        return new WebpEncoder {
            Method = WebpEncodingMethod.Default,
            FileFormat = WebpFileFormatType.Lossless,
            TransparentColorMode = WebpTransparentColorMode.Preserve,
            //UseAlphaCompression = true,
        };
    }
}