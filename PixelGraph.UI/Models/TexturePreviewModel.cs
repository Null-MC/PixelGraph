using PixelGraph.UI.Internal;
using SixLabors.ImageSharp;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Models;

public interface ITexturePreviewModel : INotifyPropertyChanged
{
    BitmapSource? Texture {get;}
    string? MousePixel {get; set;}

    int OutlineX {get;}
    int OutlineY {get;}
    int OutlineWidth {get;}
    int OutlineHeight {get;}
    bool ShowOutline {get; set;}
    bool HasOutline {get;}

    void SetOutlineBounds(in RectangleF? region);
}

internal class TexturePreviewModel : ModelBase, ITexturePreviewModel
{
    private const float HalfPixel = 0.5f - float.Epsilon;

    private BitmapSource? _texture;
    private bool _showOutline;
    private RectangleF? outlineBounds;
    private int _outlineX, _outlineY, _outlineWidth, _outlineHeight;
    private string? _mousePixel;

    public int OutlineX => _outlineX;
    public int OutlineY => _outlineY;
    public int OutlineWidth => _outlineWidth;
    public int OutlineHeight => _outlineHeight;
    public bool HasOutlineBounds => outlineBounds.HasValue;
    public bool HasOutline => _showOutline && HasOutlineBounds;

    public BitmapSource? Texture {
        get => _texture;
        set {
            _texture = value;
            OnPropertyChanged();

            UpdateOutlineBounds();
        }
    }

    public bool ShowOutline {
        get => _showOutline;
        set {
            _showOutline = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(HasOutline));
        }
    }

    public string? MousePixel {
        get => _mousePixel;
        set {
            if (_mousePixel == value) return;
            _mousePixel = value;
            OnPropertyChanged();
        }
    }


    public void SetOutlineBounds(in RectangleF? region)
    {
        outlineBounds = region;
        UpdateOutlineBounds();
    }

    private void UpdateOutlineBounds()
    {
        if (outlineBounds.HasValue && _texture != null) {
            var outlineX = (int)(outlineBounds.Value.X * _texture.PixelWidth + HalfPixel);
            var outlineY = (int)(outlineBounds.Value.Y * _texture.PixelHeight + HalfPixel);

            _outlineX = outlineX - 1;
            _outlineY = outlineY - 1;

            _outlineWidth = GetOutlineWidth() + 2;
            _outlineHeight = GetOutlineHeight() + 2;
        }
        else {
            _outlineX = _outlineY = 0;
            _outlineWidth = _outlineHeight = 2;
        }

        OnPropertyChanged(nameof(OutlineX));
        OnPropertyChanged(nameof(OutlineY));
        OnPropertyChanged(nameof(OutlineWidth));
        OnPropertyChanged(nameof(OutlineHeight));
        OnPropertyChanged(nameof(HasOutlineBounds));
        OnPropertyChanged(nameof(HasOutline));
    }

    private int GetOutlineWidth()
    {
        if (_texture == null) return 0;
        if (!outlineBounds.HasValue) return _texture.PixelWidth;
        return (int)(outlineBounds.Value.Width * _texture.PixelWidth + HalfPixel);
    }

    private int GetOutlineHeight()
    {
        if (_texture == null) return 0;
        if (!outlineBounds.HasValue) return _texture.PixelHeight;
        return (int)(outlineBounds.Value.Height * _texture.PixelHeight + HalfPixel);
    }
}