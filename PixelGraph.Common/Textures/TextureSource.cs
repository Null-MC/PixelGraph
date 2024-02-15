using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Textures;

public class TextureSource
{
    public string? LocalFile {get; set;}
    public int FrameCount {get; set;}
    public int Width {get; set;}
    public int Height {get; set;}
    public float Gamma {get; set;}
    public int BitDepth {get; set;}
    public bool HasColor {get; set;}
    public bool HasOpacity {get; set;}


    public void PopulateMetadata(ImageFormats format, ImageInfo info)
    {
        switch (format) {
            case ImageFormats.Bitmap:
                //var bmpMeta = info.Metadata.GetBmpMetadata();

                BitDepth = 8; //(int)bmpMeta.BitsPerPixel;
                HasColor = true;
                HasOpacity = false;
                break;

            case ImageFormats.Png:
                var pngMeta = info.Metadata.GetPngMetadata();

                if (pngMeta.ColorType == PngColorType.Palette) {
                    HasOpacity = true;
                    HasColor = true;
                    BitDepth = 8;
                }
                else {
                    //HasOpacity = pngMeta.HasTransparency;
                    HasOpacity = pngMeta.ColorType is PngColorType.RgbWithAlpha or PngColorType.GrayscaleWithAlpha;

                    if (pngMeta.BitDepth.HasValue)
                        BitDepth = (int)pngMeta.BitDepth;

                    if (pngMeta.ColorType.HasValue)
                        HasColor = pngMeta.ColorType is not (PngColorType.Grayscale or PngColorType.GrayscaleWithAlpha);
                }

                if (pngMeta.Gamma > float.Epsilon)
                    Gamma = pngMeta.Gamma;

                break;

            case ImageFormats.Tga:
                var tgaMeta = info.Metadata.GetTgaMetadata();

                BitDepth = 8;
                HasOpacity = tgaMeta.AlphaChannelBits > 0;
                break;

            case ImageFormats.Jpeg:
                var jpegMeta = info.Metadata.GetJpegMetadata();

                BitDepth = 8;
                HasColor = jpegMeta.ColorType != JpegEncodingColor.Luminance;
                HasOpacity = false;
                break;

            case ImageFormats.Gif:
                //var gifMeta = info.Metadata.GetGifMetadata();

                BitDepth = 8;
                HasColor = true;
                HasOpacity = false;
                break;

            case ImageFormats.WebP:
                //var webpMeta = info.Metadata.GetWebpMetadata();

                BitDepth = 8;
                HasColor = true;
                HasOpacity = false;
                break;
        }
    }

    public Type GetPixelType()
    {
        switch (BitDepth) {
            case <= 8:
                if (HasColor)
                    return HasOpacity ? typeof(Rgba32) : typeof(Rgb24);

                return HasOpacity ? typeof(La16) : typeof(L8);

            case 16:
                if (HasColor)
                    return HasOpacity ? typeof(Rgba64) : typeof(Rgb48);

                return HasOpacity ? typeof(La32) : typeof(L16);

            default:
                throw new ApplicationException($"Unsupported image encoding! [bits={BitDepth}, color={HasColor}, alpha={HasOpacity}]");
        }
    }
}