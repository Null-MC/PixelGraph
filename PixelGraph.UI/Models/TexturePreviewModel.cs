using PixelGraph.UI.Internal;
using SixLabors.ImageSharp;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Models
{
    public interface ITexturePreviewModel : INotifyPropertyChanged
    {
        string MousePixel {get;}
    }

    internal class TexturePreviewModel : ModelBase, ITexturePreviewModel
    {
        private const float HalfPixel = 0.5f - float.Epsilon;

        private BitmapSource _texture;
        private bool _enableTiling;
        private bool _showOutline;
        private RectangleF? _outlineBounds;
        private Thickness _outlineMargin;
        private int _outlineWidth;
        private int _outlineHeight;
        private double _zoom;
        private string _mousePixel;

        public bool HasOutlineBounds => _outlineBounds != null;

        //public int OutlineX => (int)(_outlineBounds?.X ?? 0f + HalfPixel) - 1;
        //public int OutlineY => (int)(_outlineBounds?.Y ?? 0f + HalfPixel) - 1;

        public Thickness OutlineMargin => _outlineMargin;
        public int OutlineWidth => _outlineWidth;
        public int OutlineHeight => _outlineHeight;

        public BitmapSource Texture {
            get => _texture;
            set {
                _texture = value;
                OnPropertyChanged();

                UpdateOutlineBounds();
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

        public bool ShowOutline {
            get => _showOutline;
            set {
                _showOutline = value;
                OnPropertyChanged();
            }
        }

        public string MousePixel {
            get => _mousePixel;
            set {
                if (_mousePixel == value) return;
                _mousePixel = value;
                OnPropertyChanged();
            }
        }

        public RectangleF? OutlineBounds {
            get => _outlineBounds;
            set {
                _outlineBounds = value;
                OnPropertyChanged();

                UpdateOutlineBounds();
                OnPropertyChanged(nameof(HasOutlineBounds));
            }
        }


        private void UpdateOutlineBounds()
        {
            if (_outlineBounds.HasValue && _texture != null) {
                var outlineX = (int)(_outlineBounds.Value.X * _texture.PixelWidth + HalfPixel);
                var outlineY = (int)(_outlineBounds.Value.Y * _texture.PixelHeight + HalfPixel);

                _outlineMargin = new Thickness {
                    Left = outlineX - 1,
                    Top = outlineY - 1,
                };

                _outlineWidth = GetOutlineWidth() + 2;
                _outlineHeight = GetOutlineHeight() + 2;
            }
            else {
                _outlineMargin = new Thickness();
                _outlineWidth = 2;
                _outlineHeight = 2;
            }

            OnPropertyChanged(nameof(OutlineMargin));
            OnPropertyChanged(nameof(OutlineWidth));
            OnPropertyChanged(nameof(OutlineHeight));
        }

        private int GetOutlineWidth()
        {
            if (_texture == null) return 0;
            if (!_outlineBounds.HasValue) return _texture.PixelWidth;
            return (int)(_outlineBounds.Value.Width * _texture.PixelWidth + HalfPixel);
        }

        private int GetOutlineHeight()
        {
            if (_texture == null) return 0;
            if (!_outlineBounds.HasValue) return _texture.PixelHeight;
            return (int)(_outlineBounds.Value.Height * _texture.PixelHeight + HalfPixel);
        }
    }
}
