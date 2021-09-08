using PixelGraph.UI.Internal;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Models
{
    internal class TexturePreviewModel : ModelBase
    {
        private BitmapSource _texture;
        private bool _enableTiling;
        private bool _showOutline;
        private double _zoom;

        public int OutlineWidth => (_texture?.PixelWidth ?? 0) + 2;
        public int OutlineHeight => (_texture?.PixelHeight ?? 0) + 2;

        public BitmapSource Texture {
            get => _texture;
            set {
                _texture = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(OutlineWidth));
                OnPropertyChanged(nameof(OutlineHeight));
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
    }
}
