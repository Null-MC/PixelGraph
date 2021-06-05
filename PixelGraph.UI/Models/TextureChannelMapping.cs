using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.Models
{
    internal class TextureChannelMapping : ModelBase
    {
        private ResourcePackChannelProperties _channel;
        private string _textureDefault;
        private ColorChannel? _colorDefault;
        private string _samplerDefault;
        private decimal? _minValueDefault;
        private decimal? _maxValueDefault;
        private byte? _rangeMinDefault;
        private byte? _rangeMaxDefault;
        private int? _shiftDefault;
        private decimal? _powerDefault;
        private bool? _invertDefault;

        public event EventHandler DataChanged;

        public string Texture => _channel?.Texture;
        public ColorChannel? Color => _channel?.Color;
        public string Sampler => _channel?.Sampler;
        public decimal? MinValue => _channel?.MinValue;
        public decimal? MaxValue => _channel?.MaxValue;
        public byte? RangeMin => _channel?.RangeMin;
        public byte? RangeMax => _channel?.RangeMax;
        public int? Shift => _channel?.Shift;
        public decimal? Power => _channel?.Power;
        public bool? Invert => _channel?.Invert;

        public string Label {get;}

        public string EditTexture {
            get => _channel?.Texture ?? _textureDefault;
            set {
                if (_channel == null) return;
                if (_channel.Texture == value) return;
                _channel.Texture = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Texture));
                OnDataChanged();
            }
        }
        
        public ColorChannel? EditColor {
            get => _channel?.Color ?? _colorDefault;
            set {
                if (_channel == null) return;
                if (_channel.Color == value) return;
                _channel.Color = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Color));
                OnDataChanged();
            }
        }
        
        public string EditSampler {
            get => _channel?.Sampler ?? _samplerDefault;
            set {
                if (_channel == null) return;
                if (_channel.Sampler == value) return;
                _channel.Sampler = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Sampler));

                OnDataChanged();
            }
        }

        public decimal? EditMinValue {
            get => _channel?.MinValue ?? _minValueDefault;
            set {
                if (_channel == null) return;
                if (_channel.MinValue == value) return;
                _channel.MinValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MinValue));
                OnDataChanged();
            }
        }
        
        public decimal? EditMaxValue {
            get => _channel?.MaxValue ?? _maxValueDefault;
            set {
                if (_channel == null) return;
                if (_channel.MaxValue == value) return;
                _channel.MaxValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaxValue));
                OnDataChanged();
            }
        }

        public byte? EditRangeMin {
            get => _channel?.RangeMin ?? _rangeMinDefault;
            set {
                if (_channel == null) return;
                if (_channel.RangeMin == value) return;
                _channel.RangeMin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RangeMin));
                OnDataChanged();
            }
        }

        public byte? EditRangeMax {
            get => _channel?.RangeMax ?? _rangeMaxDefault;
            set {
                if (_channel == null) return;
                if (_channel.RangeMax == value) return;
                _channel.RangeMax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RangeMax));
                OnDataChanged();
            }
        }

        public int? EditShift {
            get => _channel?.Shift ?? _shiftDefault;
            set {
                if (_channel == null) return;
                if (_channel.Shift == value) return;
                _channel.Shift = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Shift));
                OnDataChanged();
            }
        }

        public decimal? EditPower {
            get => _channel?.Power ?? _powerDefault;
            set {
                if (_channel == null) return;
                if (_channel.Power == value) return;
                _channel.Power = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Power));
                OnDataChanged();
            }
        }

        public bool? EditInvert {
            get => _channel?.Invert ?? _invertDefault ?? false;
            set {
                if (_channel == null) return;
                if (_channel.Invert == value) return;
                _channel.Invert = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Invert));
                OnDataChanged();
            }
        }

        public string TextureDefault {
            get => _textureDefault;
            set {
                if (_textureDefault == value) return;
                _textureDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditTexture));
            }
        }

        public ColorChannel? ColorDefault {
            get => _colorDefault;
            set {
                if (_colorDefault == value) return;
                _colorDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditColor));
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
                OnPropertyChanged(nameof(EditMinValue));
            }
        }

        public decimal? MaxValueDefault {
            get => _maxValueDefault;
            set {
                if (_maxValueDefault == value) return;
                _maxValueDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditMaxValue));
            }
        }

        public byte? RangeMinDefault {
            get => _rangeMinDefault;
            set {
                if (_rangeMinDefault == value) return;
                _rangeMinDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditRangeMin));
            }
        }

        public byte? RangeMaxDefault {
            get => _rangeMaxDefault;
            set {
                if (_rangeMaxDefault == value) return;
                _rangeMaxDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditRangeMax));
            }
        }

        public int? ShiftDefault {
            get => _shiftDefault;
            set {
                if (_shiftDefault == value) return;
                _shiftDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditShift));
            }
        }

        public decimal? PowerDefault {
            get => _powerDefault;
            set {
                if (_powerDefault == value) return;
                _powerDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditPower));
            }
        }

        public bool? InvertDefault {
            get => _invertDefault;
            set {
                if (_invertDefault == value) return;
                _invertDefault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditInvert));
            }
        }


        public TextureChannelMapping(string label)
        {
            Label = label;
        }

        public void SetChannel(ResourcePackChannelProperties channel)
        {
            _channel = channel;

            OnPropertyChanged(nameof(Texture));
            OnPropertyChanged(nameof(EditTexture));
            OnPropertyChanged(nameof(Color));
            OnPropertyChanged(nameof(EditColor));
            OnPropertyChanged(nameof(Sampler));
            OnPropertyChanged(nameof(EditSampler));
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(EditMinValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(EditMaxValue));
            OnPropertyChanged(nameof(RangeMin));
            OnPropertyChanged(nameof(EditRangeMin));
            OnPropertyChanged(nameof(RangeMax));
            OnPropertyChanged(nameof(EditRangeMax));
            OnPropertyChanged(nameof(Shift));
            OnPropertyChanged(nameof(EditShift));
            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(EditPower));
            OnPropertyChanged(nameof(Invert));
            OnPropertyChanged(nameof(EditInvert));
        }

        public void ApplyDefaultValues(ResourcePackChannelProperties encodingDefaults, string sampler = null)
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
            InvertDefault = encodingDefaults?.Invert;
        }

        public void Clear()
        {
            EditTexture = null;
            EditColor = null;
            EditSampler = null;
            EditMinValue = null;
            EditMaxValue = null;
            EditRangeMin = null;
            EditRangeMax = null;
            EditShift = null;
            EditPower = null;
            EditInvert = null;
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
