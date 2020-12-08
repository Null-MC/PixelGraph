using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class PackInput2VM : ViewModelBase
    {
        public string RootDirectory {get; set;}
        public EncodingChannelMapping[] Channels {get;}

        public EncodingChannelMapping Alpha {get;}

        public EncodingChannelMapping DiffuseRed {get;}
        public EncodingChannelMapping DiffuseGreen {get;}
        public EncodingChannelMapping DiffuseBlue {get;}

        public EncodingChannelMapping AlbedoRed {get;}
        public EncodingChannelMapping AlbedoGreen {get;}
        public EncodingChannelMapping AlbedoBlue {get;}

        public EncodingChannelMapping Height {get; set;}

        public EncodingChannelMapping Occlusion {get; set;}

        public EncodingChannelMapping NormalX {get; set;}
        public EncodingChannelMapping NormalY {get; set;}
        public EncodingChannelMapping NormalZ {get; set;}

        public EncodingChannelMapping Specular {get; set;}

        public EncodingChannelMapping Smooth {get; set;}
        public EncodingChannelMapping Rough {get; set;}

        public EncodingChannelMapping Metal {get; set;}

        public EncodingChannelMapping Porosity {get; set;}

        public EncodingChannelMapping SSS {get; set;}

        public EncodingChannelMapping Emissive {get; set;}

        private ResourcePackInputProperties _packInput;

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                if (_packInput == value) return;
                _packInput = value;
                OnPropertyChanged();

                UpdateChannels();
                OnFormatChanged();
            }
        }

        public string Format {
            get => PackInput?.Format;
            set {
                if (PackInput == null) return;
                if (PackInput.Format == value) return;
                PackInput.Format = value;
                OnPropertyChanged();

                OnFormatChanged();
            }
        }


        public PackInput2VM()
        {
            Channels = new []{
                Alpha = new EncodingChannelMapping("Alpha"),

                DiffuseRed = new EncodingChannelMapping("Diffuse Red"),
                DiffuseGreen = new EncodingChannelMapping("Diffuse Green"),
                DiffuseBlue = new EncodingChannelMapping("Diffuse Blue"),

                AlbedoRed = new EncodingChannelMapping("Albedo Red"),
                AlbedoGreen = new EncodingChannelMapping("Albedo Green"),
                AlbedoBlue = new EncodingChannelMapping("Albedo Blue"),

                Height = new EncodingChannelMapping("Height"),

                Occlusion = new EncodingChannelMapping("Ambient Occlusion"),

                NormalX = new EncodingChannelMapping("Normal-X"),
                NormalY = new EncodingChannelMapping("Normal-Y"),
                NormalZ = new EncodingChannelMapping("Normal-Z"),

                Specular = new EncodingChannelMapping("Specular"),

                Smooth = new EncodingChannelMapping("Smooth"),
                Rough = new EncodingChannelMapping("Rough"),

                Metal = new EncodingChannelMapping("Metal"),

                Porosity = new EncodingChannelMapping("Porosity"),

                SSS = new EncodingChannelMapping("Sub-Surface-Scattering"),

                Emissive = new EncodingChannelMapping("Emissive"),
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

        private void OnFormatChanged()
        {
            // TODO: save input

            UpdateDefaultValues();
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

    internal class EncodingChannelMapping : ViewModelBase
    {
        private ResourcePackChannelProperties _channel;
        private string _textureDefault;
        private ColorChannel? _colorDefault;
        private string _samplerDefault;
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

        public string Sampler {
            get => _channel?.Sampler;
            set {
                if (_channel == null) return;
                if (_channel.Sampler == value) return;
                _channel.Sampler = value;
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

        public string SamplerDefault {
            get => _samplerDefault;
            set {
                if (_samplerDefault == value) return;
                _samplerDefault = value;
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


        public EncodingChannelMapping(string label)
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
            OnPropertyChanged(nameof(Shift));
            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(Invert));
        }

        public void ApplyDefaultValues(ResourcePackChannelProperties encodingDefaults)
        {
            TextureDefault = encodingDefaults.Texture;
            ColorDefault = encodingDefaults.Color;
            SamplerDefault = encodingDefaults.Sampler;
            MinDefault = encodingDefaults.MinValue;
            MaxDefault = encodingDefaults.MaxValue;
            ShiftDefault = encodingDefaults.Shift;
            PowerDefault = encodingDefaults.Power;
            InvertDefault = encodingDefaults.Invert;
        }
    }
}
