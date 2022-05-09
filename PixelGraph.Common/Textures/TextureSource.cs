using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace PixelGraph.Common.Textures
{
    public class TextureSource
    {
        public string LocalFile {get; set;}
        public int FrameCount {get; set;}
        public int Width {get; set;}
        public int Height {get; set;}
        public float Gamma {get; set;}
        public int BitDepth {get; set;}
        public bool HasColor {get; set;}
        public bool HasOpacity {get; set;}


        public void PopulateMetadata(ImageFormats format, IImageInfo info)
        {
            switch (format) {
                case ImageFormats.Bitmap:
                    //var bmpMeta = info.Metadata.GetBmpMetadata();

                    BitDepth = 8; //(int)bmpMeta.BitsPerPixel;
                    break;

                case ImageFormats.Png:
                    var pngMeta = info.Metadata.GetPngMetadata();

                    //HasOpacity = pngMeta.HasTransparency;
                    HasOpacity = pngMeta.ColorType is PngColorType.RgbWithAlpha or PngColorType.GrayscaleWithAlpha;

                    if (pngMeta.BitDepth.HasValue)
                        BitDepth = (int)pngMeta.BitDepth;

                    if (pngMeta.Gamma > float.Epsilon)
                        Gamma = pngMeta.Gamma;

                    if (pngMeta.ColorType.HasValue)
                        HasColor = pngMeta.ColorType is not (PngColorType.Grayscale or PngColorType.GrayscaleWithAlpha);

                    break;

                case ImageFormats.Tga:
                    var tgaMeta = info.Metadata.GetTgaMetadata();

                    BitDepth = 8;
                    HasOpacity = tgaMeta.AlphaChannelBits > 0;
                    break;

                case ImageFormats.Jpeg:
                    var jpegMeta = info.Metadata.GetJpegMetadata();

                    BitDepth = 8;
                    HasColor = jpegMeta.ColorType == JpegColorType.Rgb;
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
    }
}
