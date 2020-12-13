using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels
{
    internal class PackProfilesVM : ViewModelBase
    {
        protected ObservableCollection<ProfileItem> _profiles;
        private ResourcePackProfileProperties _loadedProfile;
        private ProfileItem _selectedProfileItem;
        private string _rootDirectory;

        public event EventHandler DataChanged;

        public OutputChannelMapping[] EncodingChannels {get;}

        public OutputChannelMapping Alpha {get;}

        public OutputChannelMapping DiffuseRed {get;}
        public OutputChannelMapping DiffuseGreen {get;}
        public OutputChannelMapping DiffuseBlue {get;}

        public OutputChannelMapping AlbedoRed {get;}
        public OutputChannelMapping AlbedoGreen {get;}
        public OutputChannelMapping AlbedoBlue {get;}

        public OutputChannelMapping Height {get; set;}

        public OutputChannelMapping Occlusion {get; set;}

        public OutputChannelMapping NormalX {get; set;}
        public OutputChannelMapping NormalY {get; set;}
        public OutputChannelMapping NormalZ {get; set;}

        public OutputChannelMapping Specular {get; set;}

        public OutputChannelMapping Smooth {get; set;}
        public OutputChannelMapping Rough {get; set;}

        public OutputChannelMapping Metal {get; set;}

        public OutputChannelMapping Porosity {get; set;}

        public OutputChannelMapping SSS {get; set;}

        public OutputChannelMapping Emissive {get; set;}

        public bool HasSelectedProfile => _selectedProfileItem != null;
        public bool HasLoadedProfile => _loadedProfile != null;

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProfileItem> Profiles {
            get => _profiles;
            set {
                _profiles = value;
                OnPropertyChanged();
            }
        }

        public ResourcePackProfileProperties LoadedProfile {
            get => _loadedProfile;
            set {
                _loadedProfile = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasLoadedProfile));
                OnPropertyChanged(nameof(GameEdition));
                OnPropertyChanged(nameof(PackFormat));
                OnPropertyChanged(nameof(PackDescription));
                OnPropertyChanged(nameof(PackTags));
                OnPropertyChanged(nameof(EncodingFormat));
                OnPropertyChanged(nameof(EncodingSampler));
                // TODO: ...

                UpdateChannels();
                UpdateDefaultValues();
            }
        }

        public ProfileItem SelectedProfileItem {
            get => _selectedProfileItem;
            set {
                _selectedProfileItem = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasSelectedProfile));
            }
        }

        public string GameEdition {
            get => _loadedProfile?.Edition;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Edition = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public int? PackFormat {
            get => _loadedProfile?.Format;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Format = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public string PackDescription {
            get => _loadedProfile?.Description;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Description = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public string PackTags {
            get => _loadedProfile?.Tags;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Tags = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public string EncodingFormat {
            get => _loadedProfile?.Output?.Format;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Output ??= new ResourcePackOutputProperties();
                _loadedProfile.Output.Format = value;
                OnPropertyChanged();

                UpdateDefaultValues();
                OnDataChanged();
            }
        }

        public string EncodingSampler {
            get => _loadedProfile?.Output?.Sampler;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Output ??= new ResourcePackOutputProperties();
                _loadedProfile.Output.Sampler = value;
                OnPropertyChanged();

                UpdateDefaultValues();
                OnDataChanged();
            }
        }


        public PackProfilesVM()
        {
            _profiles = new ObservableCollection<ProfileItem>();

            EncodingChannels = new []{
                Alpha = new OutputChannelMapping("Alpha"),

                DiffuseRed = new OutputChannelMapping("Diffuse Red"),
                DiffuseGreen = new OutputChannelMapping("Diffuse Green"),
                DiffuseBlue = new OutputChannelMapping("Diffuse Blue"),

                AlbedoRed = new OutputChannelMapping("Albedo Red"),
                AlbedoGreen = new OutputChannelMapping("Albedo Green"),
                AlbedoBlue = new OutputChannelMapping("Albedo Blue"),

                Height = new OutputChannelMapping("Height"),

                Occlusion = new OutputChannelMapping("Ambient Occlusion"),

                NormalX = new OutputChannelMapping("Normal-X"),
                NormalY = new OutputChannelMapping("Normal-Y"),
                NormalZ = new OutputChannelMapping("Normal-Z"),

                Specular = new OutputChannelMapping("Specular"),

                Smooth = new OutputChannelMapping("Smooth"),
                Rough = new OutputChannelMapping("Rough"),

                Metal = new OutputChannelMapping("Metal"),

                Porosity = new OutputChannelMapping("Porosity"),

                SSS = new OutputChannelMapping("Sub-Surface-Scattering"),

                Emissive = new OutputChannelMapping("Emissive"),
            };
        }

        private void UpdateChannels()
        {
            Alpha.SetChannel(_loadedProfile?.Output?.Alpha);

            DiffuseRed.SetChannel(_loadedProfile?.Output?.DiffuseRed);
            DiffuseGreen.SetChannel(_loadedProfile?.Output?.DiffuseGreen);
            DiffuseBlue.SetChannel(_loadedProfile?.Output?.DiffuseBlue);

            AlbedoRed.SetChannel(_loadedProfile?.Output?.AlbedoRed);
            AlbedoGreen.SetChannel(_loadedProfile?.Output?.AlbedoGreen);
            AlbedoBlue.SetChannel(_loadedProfile?.Output?.AlbedoBlue);

            Height.SetChannel(_loadedProfile?.Output?.Height);
            Occlusion.SetChannel(_loadedProfile?.Output?.Occlusion);

            NormalX.SetChannel(_loadedProfile?.Output?.NormalX);
            NormalY.SetChannel(_loadedProfile?.Output?.NormalY);
            NormalZ.SetChannel(_loadedProfile?.Output?.NormalZ);

            Specular.SetChannel(_loadedProfile?.Output?.Specular);

            Smooth.SetChannel(_loadedProfile?.Output?.Smooth);
            Rough.SetChannel(_loadedProfile?.Output?.Rough);

            Metal.SetChannel(_loadedProfile?.Output?.Rough);

            Porosity.SetChannel(_loadedProfile?.Output?.Rough);

            SSS.SetChannel(_loadedProfile?.Output?.SSS);

            Emissive.SetChannel(_loadedProfile?.Output?.Rough);
        }

        public void UpdateDefaultValues()
        {
            var encoding = TextureEncoding.GetFactory(_loadedProfile?.Output?.Format);
            var encodingDefaults = encoding?.Create();
            var sampler = _loadedProfile?.Output?.Sampler ?? Samplers.Point;

            Alpha.ApplyDefaultValues(encodingDefaults?.Alpha, sampler);

            DiffuseRed.ApplyDefaultValues(encodingDefaults?.DiffuseRed, sampler);
            DiffuseGreen.ApplyDefaultValues(encodingDefaults?.DiffuseGreen, sampler);
            DiffuseBlue.ApplyDefaultValues(encodingDefaults?.DiffuseBlue, sampler);

            AlbedoRed.ApplyDefaultValues(encodingDefaults?.AlbedoRed, sampler);
            AlbedoGreen.ApplyDefaultValues(encodingDefaults?.AlbedoGreen, sampler);
            AlbedoBlue.ApplyDefaultValues(encodingDefaults?.AlbedoBlue, sampler);

            Height.ApplyDefaultValues(encodingDefaults?.Height, sampler);
            Occlusion.ApplyDefaultValues(encodingDefaults?.Occlusion, sampler);

            NormalX.ApplyDefaultValues(encodingDefaults?.NormalX, sampler);
            NormalY.ApplyDefaultValues(encodingDefaults?.NormalY, sampler);
            NormalZ.ApplyDefaultValues(encodingDefaults?.NormalZ, sampler);

            Specular.ApplyDefaultValues(encodingDefaults?.Specular, sampler);

            Smooth.ApplyDefaultValues(encodingDefaults?.Smooth, sampler);
            Rough.ApplyDefaultValues(encodingDefaults?.Rough, sampler);

            Metal.ApplyDefaultValues(encodingDefaults?.Metal, sampler);

            Porosity.ApplyDefaultValues(encodingDefaults?.Porosity, sampler);

            SSS.ApplyDefaultValues(encodingDefaults?.SSS, sampler);

            Emissive.ApplyDefaultValues(encodingDefaults?.Emissive, sampler);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class ProfilesDesignVM : PackProfilesVM
    {
        public ProfilesDesignVM()
        {
            _profiles.Add(new ProfileItem {Name = "Profile A"});
            _profiles.Add(new ProfileItem {Name = "Profile B"});
            _profiles.Add(new ProfileItem {Name = "Profile C"});

            SelectedProfileItem = _profiles[0];

            LoadedProfile = new ResourcePackProfileProperties {
                Edition = "Java",
                Description = "Designer Data",
                Format = 99,
                ImageEncoding = "tga",
                Output = {
                    Sampler = "point",
                },
            };
        }
    }

    internal class OutputChannelMapping : ViewModelBase
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
            OnPropertyChanged(nameof(Shift));
            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(Invert));
        }

        public void ApplyDefaultValues(ResourcePackChannelProperties encodingDefaults, string sampler)
        {
            TextureDefault = encodingDefaults?.Texture;
            ColorDefault = encodingDefaults?.Color;
            SamplerDefault = encodingDefaults?.Sampler ?? sampler;
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
            Sampler = null;
            MinValue = null;
            MaxValue = null;
            Shift = null;
            Power = null;
            Invert = null;
        }
    }
}
