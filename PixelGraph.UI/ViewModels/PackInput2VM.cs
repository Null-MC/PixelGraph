using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class PackInput2VM : ViewModelBase
    {
        public string RootDirectory {get; set;}
        public InputChannelMapping[] Channels {get;}

        public InputChannelMapping Alpha {get;}

        public InputChannelMapping DiffuseRed {get;}
        public InputChannelMapping DiffuseGreen {get;}
        public InputChannelMapping DiffuseBlue {get;}

        public InputChannelMapping AlbedoRed {get;}
        public InputChannelMapping AlbedoGreen {get;}
        public InputChannelMapping AlbedoBlue {get;}

        public InputChannelMapping Height {get; set;}

        public InputChannelMapping Occlusion {get; set;}

        public InputChannelMapping NormalX {get; set;}
        public InputChannelMapping NormalY {get; set;}
        public InputChannelMapping NormalZ {get; set;}

        public InputChannelMapping Specular {get; set;}

        public InputChannelMapping Smooth {get; set;}
        public InputChannelMapping Rough {get; set;}

        public InputChannelMapping Metal {get; set;}

        public InputChannelMapping Porosity {get; set;}

        public InputChannelMapping SSS {get; set;}

        public InputChannelMapping Emissive {get; set;}

        private ResourcePackInputProperties _packInput;

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                if (_packInput == value) return;
                _packInput = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(Format));
                UpdateChannels();
                UpdateDefaultValues();
            }
        }

        public string Format {
            get => PackInput?.Format;
            set {
                if (PackInput == null) return;
                if (PackInput.Format == value) return;
                PackInput.Format = value;
                OnPropertyChanged();

                UpdateDefaultValues();
            }
        }


        public PackInput2VM()
        {
            Channels = new []{
                Alpha = new InputChannelMapping("Alpha"),

                DiffuseRed = new InputChannelMapping("Diffuse Red"),
                DiffuseGreen = new InputChannelMapping("Diffuse Green"),
                DiffuseBlue = new InputChannelMapping("Diffuse Blue"),

                AlbedoRed = new InputChannelMapping("Albedo Red"),
                AlbedoGreen = new InputChannelMapping("Albedo Green"),
                AlbedoBlue = new InputChannelMapping("Albedo Blue"),

                Height = new InputChannelMapping("Height"),

                Occlusion = new InputChannelMapping("Ambient Occlusion"),

                NormalX = new InputChannelMapping("Normal-X"),
                NormalY = new InputChannelMapping("Normal-Y"),
                NormalZ = new InputChannelMapping("Normal-Z"),

                Specular = new InputChannelMapping("Specular"),

                Smooth = new InputChannelMapping("Smooth"),
                Rough = new InputChannelMapping("Rough"),

                Metal = new InputChannelMapping("Metal"),

                Porosity = new InputChannelMapping("Porosity"),

                SSS = new InputChannelMapping("Sub-Surface-Scattering"),

                Emissive = new InputChannelMapping("Emissive"),
            };
        }

        public void UpdateDefaultValues()
        {
            var encoding = TextureEncoding.GetFactory(_packInput?.Format);
            var encodingDefaults = encoding?.Create();

            Alpha.ApplyDefaultValues(encodingDefaults?.Alpha);

            DiffuseRed.ApplyDefaultValues(encodingDefaults?.DiffuseRed);
            DiffuseGreen.ApplyDefaultValues(encodingDefaults?.DiffuseGreen);
            DiffuseBlue.ApplyDefaultValues(encodingDefaults?.DiffuseBlue);

            AlbedoRed.ApplyDefaultValues(encodingDefaults?.AlbedoRed);
            AlbedoGreen.ApplyDefaultValues(encodingDefaults?.AlbedoGreen);
            AlbedoBlue.ApplyDefaultValues(encodingDefaults?.AlbedoBlue);

            Height.ApplyDefaultValues(encodingDefaults?.Height);
            Occlusion.ApplyDefaultValues(encodingDefaults?.Occlusion);

            NormalX.ApplyDefaultValues(encodingDefaults?.NormalX);
            NormalY.ApplyDefaultValues(encodingDefaults?.NormalY);
            NormalZ.ApplyDefaultValues(encodingDefaults?.NormalZ);

            Specular.ApplyDefaultValues(encodingDefaults?.Specular);

            Smooth.ApplyDefaultValues(encodingDefaults?.Smooth);
            Rough.ApplyDefaultValues(encodingDefaults?.Rough);

            Metal.ApplyDefaultValues(encodingDefaults?.Metal);

            Porosity.ApplyDefaultValues(encodingDefaults?.Porosity);

            SSS.ApplyDefaultValues(encodingDefaults?.SSS);

            Emissive.ApplyDefaultValues(encodingDefaults?.Emissive);
        }

        private void UpdateChannels()
        {
            Alpha.SetChannel(_packInput?.Alpha);

            DiffuseRed.SetChannel(_packInput?.DiffuseRed);
            DiffuseGreen.SetChannel(_packInput?.DiffuseGreen);
            DiffuseBlue.SetChannel(_packInput?.DiffuseBlue);

            AlbedoRed.SetChannel(_packInput?.AlbedoRed);
            AlbedoGreen.SetChannel(_packInput?.AlbedoGreen);
            AlbedoBlue.SetChannel(_packInput?.AlbedoBlue);

            Height.SetChannel(_packInput?.Height);
            Occlusion.SetChannel(_packInput?.Occlusion);

            NormalX.SetChannel(_packInput?.NormalX);
            NormalY.SetChannel(_packInput?.NormalY);
            NormalZ.SetChannel(_packInput?.NormalZ);

            Specular.SetChannel(_packInput?.Specular);

            Smooth.SetChannel(_packInput?.Smooth);
            Rough.SetChannel(_packInput?.Rough);

            Metal.SetChannel(_packInput?.Rough);

            Porosity.SetChannel(_packInput?.Rough);

            SSS.SetChannel(_packInput?.SSS);

            Emissive.SetChannel(_packInput?.Rough);
        }
    }

    internal class PackInput2DesignVM : PackInput2VM
    {
        public PackInput2DesignVM()
        {
            PackInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Raw,
                Alpha = {
                    MinValue = 100,
                },
            };
        }
    }

    internal class InputChannelMapping : ViewModelBase
    {
        private ResourcePackChannelProperties _channel;
        private string _textureDefault;
        private ColorChannel? _colorDefault;
        private byte? _minDefault;
        private byte? _maxDefault;
        private short? _shiftDefault;
        private decimal? _powerDefault;
        private bool? _invertDefault;

        public string Label {get;}

        public string Texture {
            get => _channel?.Texture;
            set {
                if (_channel == null) return;
                if (_channel.Texture == value) return;
                _channel.Texture = value;
                OnPropertyChanged();
            }
        }

        public ColorChannel? Color {
            get => _channel?.Color;
            set {
                if (_channel == null) return;
                if (_channel.Color == value) return;
                _channel.Color = value;
                OnPropertyChanged();
            }
        }

        public byte? MinValue {
            get => _channel?.MinValue;
            set {
                if (_channel == null) return;
                if (_channel.MinValue == value) return;
                _channel.MinValue = value;
                OnPropertyChanged();
            }
        }

        public byte? MaxValue {
            get => _channel?.MaxValue;
            set {
                if (_channel == null) return;
                if (_channel.MaxValue == value) return;
                _channel.MaxValue = value;
                OnPropertyChanged();
            }
        }

        public short? Shift {
            get => _channel?.Shift;
            set {
                if (_channel == null) return;
                if (_channel.Shift == value) return;
                _channel.Shift = value;
                OnPropertyChanged();
            }
        }

        public decimal? Power {
            get => _channel?.Power;
            set {
                if (_channel == null) return;
                if (_channel.Power == value) return;
                _channel.Power = value;
                OnPropertyChanged();
            }
        }

        public bool? Invert {
            get => _channel?.Invert;
            set {
                if (_channel == null) return;
                if (_channel.Invert == value) return;
                _channel.Invert = value;
                OnPropertyChanged();
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

        public byte? MinDefault {
            get => _minDefault;
            set {
                if (_minDefault == value) return;
                _minDefault = value;
                OnPropertyChanged();
            }
        }

        public byte? MaxDefault {
            get => _maxDefault;
            set {
                if (_maxDefault == value) return;
                _maxDefault = value;
                OnPropertyChanged();
            }
        }

        public short? ShiftDefault {
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

        public bool? InvertDefault {
            get => _invertDefault;
            set {
                if (_invertDefault == value) return;
                _invertDefault = value;
                OnPropertyChanged();
            }
        }


        public InputChannelMapping(string label)
        {
            Label = label;
        }

        public void SetChannel(ResourcePackChannelProperties channel)
        {
            _channel = channel;

            OnPropertyChanged(nameof(Texture));
            OnPropertyChanged(nameof(Color));
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(Shift));
            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(Invert));
        }

        public void ApplyDefaultValues(ResourcePackChannelProperties encodingDefaults)
        {
            TextureDefault = encodingDefaults?.Texture;
            ColorDefault = encodingDefaults?.Color;
            MinDefault = encodingDefaults?.MinValue;
            MaxDefault = encodingDefaults?.MaxValue;
            ShiftDefault = encodingDefaults?.Shift;
            PowerDefault = encodingDefaults?.Power;
            InvertDefault = encodingDefaults?.Invert;
        }

        public void Clear()
        {
            Texture = null;
            Color = null;
            MinValue = null;
            MaxValue = null;
            Shift = null;
            Power = null;
            Invert = null;
        }
    }
}
