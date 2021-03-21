using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class OutputChannelMapping : ViewModelBase
    {
        private ResourcePackChannelProperties _channel;
        private string _textureDefault;
        private ColorChannel? _colorDefault;
        private string _samplerDefault;
        private decimal? _minValueDefault;
        private decimal? _maxValueDefault;
        private byte? _rangeMinDefault;
        private byte? _rangeMaxDefault;
        private decimal? _shiftDefault;
        private decimal? _powerDefault;
        //private bool? _perceptualDefault;
        private bool? _invertDefault;

        public event EventHandler DataChanged;

        public string Label {get;}

        public string Texture {
            get => _channel?.Texture;
            set {
                if (_channel == null) return;
                if (_channel.Texture == value) return;
                _channel.Texture = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public ColorChannel? Color {
            get => _channel?.Color;
            set {
                if (_channel == null) return;
                if (_channel.Color == value) return;
                _channel.Color = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public string Sampler {
            get => _channel?.Sampler;
            set {
                if (_channel == null) return;
                if (_channel.Sampler == value) return;
                _channel.Sampler = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public decimal? MinValue {
            get => _channel?.MinValue;
            set {
                if (_channel == null) return;
                if (_channel.MinValue == value) return;
                _channel.MinValue = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public decimal? MaxValue {
            get => _channel?.MaxValue;
            set {
                if (_channel == null) return;
                if (_channel.MaxValue == value) return;
                _channel.MaxValue = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public byte? RangeMin {
            get => _channel?.RangeMin;
            set {
                if (_channel == null) return;
                if (_channel.RangeMin == value) return;
                _channel.RangeMin = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public byte? RangeMax {
            get => _channel?.RangeMax;
            set {
                if (_channel == null) return;
                if (_channel.RangeMax == value) return;
                _channel.RangeMax = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public int? Shift {
            get => _channel?.Shift;
            set {
                if (_channel == null) return;
                if (_channel.Shift == value) return;
                _channel.Shift = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public decimal? Power {
            get => _channel?.Power;
            set {
                if (_channel == null) return;
                if (_channel.Power == value) return;
                _channel.Power = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        //public bool? Perceptual {
        //    get => _channel?.Perceptual;
        //    set {
        //        if (_channel == null) return;
        //        if (_channel.Perceptual == value) return;
        //        _channel.Perceptual = value;
        //        OnPropertyChanged();

        //        OnDataChanged();
        //    }
        //}

        public bool? Invert {
            get => _channel?.Invert;
            set {
                if (_channel == null) return;
                if (_channel.Invert == value) return;
                _channel.Invert = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public string TextureDefault {
            get => _textureDefault;
            set {
                if (_textureDefault == value) return;
                _textureDefault = value;
                OnPropertyChanged();
            }
        }

        public ColorChannel? ColorDefault {
            get => _colorDefault;
            set {
                if (_colorDefault == value) return;
                _colorDefault = value;
                OnPropertyChanged();
            }
        }

        public string SamplerDefault {
            get => _samplerDefault;
            set {
                if (_samplerDefault == value) return;
                _samplerDefault = value;
                OnPropertyChanged();
            }
        }

        public decimal? MinValueDefault {
            get => _minValueDefault;
            set {
                if (_minValueDefault == value) return;
                _minValueDefault = value;
                OnPropertyChanged();
            }
        }

        public decimal? MaxValueDefault {
            get => _maxValueDefault;
            set {
                if (_maxValueDefault == value) return;
                _maxValueDefault = value;
                OnPropertyChanged();
            }
        }

        public byte? RangeMinDefault {
            get => _rangeMinDefault;
            set {
                if (_rangeMinDefault == value) return;
                _rangeMinDefault = value;
                OnPropertyChanged();
            }
        }

        public byte? RangeMaxDefault {
            get => _rangeMaxDefault;
            set {
                if (_rangeMaxDefault == value) return;
                _rangeMaxDefault = value;
                OnPropertyChanged();
            }
        }

        public decimal? ShiftDefault {
            get => _shiftDefault;
            set {
                if (_shiftDefault == value) return;
                _shiftDefault = value;
                OnPropertyChanged();
            }
        }

        public decimal? PowerDefault {
            get => _powerDefault;
            set {
                if (_powerDefault == value) return;
                _powerDefault = value;
                OnPropertyChanged();
            }
        }

        //public bool? PerceptualDefault {
        //    get => _perceptualDefault;
        //    set {
        //        if (_perceptualDefault == value) return;
        //        _perceptualDefault = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool? InvertDefault {
            get => _invertDefault;
            set {
                if (_invertDefault == value) return;
                _invertDefault = value;
                OnPropertyChanged();
            }
        }


        public OutputChannelMapping(string label)
        {
            Label = label;
        }

        public void SetChannel(ResourcePackChannelProperties channel)
        {
            _channel = channel;

            OnPropertyChanged(nameof(Texture));
            OnPropertyChanged(nameof(Color));
            OnPropertyChanged(nameof(Sampler));
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(RangeMin));
            OnPropertyChanged(nameof(RangeMax));
            OnPropertyChanged(nameof(Shift));
            OnPropertyChanged(nameof(Power));
            //OnPropertyChanged(nameof(Perceptual));
            OnPropertyChanged(nameof(Invert));
        }

        public void ApplyDefaultValues(ResourcePackChannelProperties encodingDefaults, string sampler)
        {
            TextureDefault = encodingDefaults?.Texture;
            ColorDefault = encodingDefaults?.Color;
            SamplerDefault = encodingDefaults?.Sampler ?? sampler;
            MinValueDefault = encodingDefaults?.MinValue;
            MaxValueDefault = encodingDefaults?.MaxValue;
            RangeMinDefault = encodingDefaults?.RangeMin;
            RangeMaxDefault = encodingDefaults?.RangeMax;
            ShiftDefault = encodingDefaults?.Shift;
            PowerDefault = encodingDefaults?.Power;
            //PerceptualDefault = encodingDefaults?.Perceptual;
            InvertDefault = encodingDefaults?.Invert;
        }

        public void Clear()
        {
            Texture = null;
            Color = null;
            Sampler = null;
            MinValue = null;
            MaxValue = null;
            RangeMin = null;
            RangeMax = null;
            Shift = null;
            Power = null;
            //Perceptual = null;
            Invert = null;
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
