using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using SixLabors.ImageSharp.PixelFormats;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace PixelGraph.UI.ViewModels;

internal class TexturePreviewViewModel : ModelBase
{
    private ITexturePreviewModel? _texturePreviewData;
    private bool _enableTiling;
    private double _zoom = 1d;

    public ITexturePreviewModel? TexturePreviewData {
        get => _texturePreviewData;
        set {
            _texturePreviewData = value;
            OnPropertyChanged();
        }
    }
        
    public double Zoom {
        get => _zoom;
        set {
            _zoom = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(ZoomText));
        }
    }

    public string ZoomText {
        get => ZoomHelper.Format(_zoom);
        set {
            _zoom = ZoomHelper.Parse(value);
            OnPropertyChanged();

            OnPropertyChanged(nameof(Zoom));
        }
    }

    public bool EnableTiling {
        get => _enableTiling;
        set {
            _enableTiling = value;
            OnPropertyChanged();
        }
    }


    public void UpdateMouseColor(in Point pos)
    {
        if (TexturePreviewData?.Texture == null) {
            // warn
            return;
        }

        var rect = new Int32Rect((int)pos.X, (int)pos.Y, 1, 1);
        rect.X = Math.Clamp(rect.X, 0, TexturePreviewData.Texture.PixelWidth-1);
        rect.Y = Math.Clamp(rect.Y, 0, TexturePreviewData.Texture.PixelHeight-1);

        var bytesPerPixel = (TexturePreviewData.Texture.Format.BitsPerPixel + 7) / 8;

        var bytes = new byte[bytesPerPixel];
        TexturePreviewData.Texture.CopyPixels(rect, bytes, bytesPerPixel, 0);

        if (TexturePreviewData.Texture is ImageSharpSource imageSharpTex) {
            if (imageSharpTex.SourceFormat == typeof(Rgb24)) {
                var hex = ColorHelper.ToHexRGB(bytes[2], bytes[1], bytes[0]);
                TexturePreviewData.MousePixel = $"C={hex}";
            }
            else if (imageSharpTex.SourceFormat == typeof(L8)) {
                TexturePreviewData.MousePixel = $"V={bytes[0]}";
            }
            else {
                TexturePreviewData.MousePixel = null;
            }
        }
        else {
            if (TexturePreviewData.Texture.Format == PixelFormats.Bgra32 || TexturePreviewData.Texture.Format == PixelFormats.Bgr32) {
                var hex = ColorHelper.ToHexRGB(bytes[2], bytes[1], bytes[0]);
                TexturePreviewData.MousePixel = $"C={hex}";
            }
            else if (TexturePreviewData.Texture.Format == PixelFormats.Gray8 || TexturePreviewData.Texture.Format == PixelFormats.Gray16) {
                TexturePreviewData.MousePixel = $"V={bytes[0]}";
            }
            else {
                TexturePreviewData.MousePixel = null;
            }
        }
    }
}

internal class TexturePreviewDesignerViewModel : TexturePreviewViewModel {}