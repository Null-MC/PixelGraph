using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using System;
using System.Collections.ObjectModel;
using PixelGraph.Common.Material;
using PixelGraph.UI.ViewData;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishProfilesVM : ViewModelBase
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
        public OutputChannelMapping F0 {get; set;}

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
                OnPropertyChanged(nameof(TextureSize));
                OnPropertyChanged(nameof(BlockTextureSize));
                OnPropertyChanged(nameof(TextureScale));
                OnPropertyChanged(nameof(AutoGenerateNormal));
                OnPropertyChanged(nameof(AutoGenerateOcclusion));

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
            get => _loadedProfile?.Encoding?.Format;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Encoding ??= new ResourcePackOutputProperties();
                _loadedProfile.Encoding.Format = value;
                OnPropertyChanged();

                UpdateDefaultValues();
                OnDataChanged();
            }
        }

        public string EncodingSampler {
            get => _loadedProfile?.Encoding?.Sampler;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Encoding ??= new ResourcePackOutputProperties();
                _loadedProfile.Encoding.Sampler = value;
                OnPropertyChanged();

                UpdateDefaultValues();
                OnDataChanged();
            }
        }

        public int? TextureSize {
            get => _loadedProfile?.TextureSize;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.TextureSize = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? BlockTextureSize {
            get => _loadedProfile?.BlockTextureSize;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.BlockTextureSize = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? TextureScale {
            get => _loadedProfile?.TextureScale;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.TextureScale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? AutoGenerateNormal {
            get => _loadedProfile?.AutoGenerateNormal ?? ResourcePackProfileProperties.AutoGenerateNormalDefault;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.AutoGenerateNormal = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? AutoGenerateOcclusion {
            get => _loadedProfile?.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.AutoGenerateOcclusion = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        public PublishProfilesVM()
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
                F0 = new OutputChannelMapping("F0"),

                Porosity = new OutputChannelMapping("Porosity"),

                SSS = new OutputChannelMapping("Sub-Surface-Scattering"),

                Emissive = new OutputChannelMapping("Emissive"),
            };

            Alpha.DataChanged += OnPropertyDataChanged;
            
            DiffuseRed.DataChanged += OnPropertyDataChanged;
            DiffuseGreen.DataChanged += OnPropertyDataChanged;
            DiffuseBlue.DataChanged += OnPropertyDataChanged;

            AlbedoRed.DataChanged += OnPropertyDataChanged;
            AlbedoGreen.DataChanged += OnPropertyDataChanged;
            AlbedoBlue.DataChanged += OnPropertyDataChanged;

            Height.DataChanged += OnPropertyDataChanged;
            Occlusion.DataChanged += OnPropertyDataChanged;

            NormalX.DataChanged += OnPropertyDataChanged;
            NormalY.DataChanged += OnPropertyDataChanged;
            NormalZ.DataChanged += OnPropertyDataChanged;

            Specular.DataChanged += OnPropertyDataChanged;

            Smooth.DataChanged += OnPropertyDataChanged;
            Rough.DataChanged += OnPropertyDataChanged;

            Metal.DataChanged += OnPropertyDataChanged;
            F0.DataChanged += OnPropertyDataChanged;

            Porosity.DataChanged += OnPropertyDataChanged;

            SSS.DataChanged += OnPropertyDataChanged;

            Emissive.DataChanged += OnPropertyDataChanged;
        }

        private void UpdateChannels()
        {
            Alpha.SetChannel(_loadedProfile?.Encoding?.Alpha);

            DiffuseRed.SetChannel(_loadedProfile?.Encoding?.DiffuseRed);
            DiffuseGreen.SetChannel(_loadedProfile?.Encoding?.DiffuseGreen);
            DiffuseBlue.SetChannel(_loadedProfile?.Encoding?.DiffuseBlue);

            AlbedoRed.SetChannel(_loadedProfile?.Encoding?.AlbedoRed);
            AlbedoGreen.SetChannel(_loadedProfile?.Encoding?.AlbedoGreen);
            AlbedoBlue.SetChannel(_loadedProfile?.Encoding?.AlbedoBlue);

            Height.SetChannel(_loadedProfile?.Encoding?.Height);
            Occlusion.SetChannel(_loadedProfile?.Encoding?.Occlusion);

            NormalX.SetChannel(_loadedProfile?.Encoding?.NormalX);
            NormalY.SetChannel(_loadedProfile?.Encoding?.NormalY);
            NormalZ.SetChannel(_loadedProfile?.Encoding?.NormalZ);

            Specular.SetChannel(_loadedProfile?.Encoding?.Specular);

            Smooth.SetChannel(_loadedProfile?.Encoding?.Smooth);
            Rough.SetChannel(_loadedProfile?.Encoding?.Rough);

            Metal.SetChannel(_loadedProfile?.Encoding?.Metal);
            F0.SetChannel(_loadedProfile?.Encoding?.F0);

            Porosity.SetChannel(_loadedProfile?.Encoding?.Porosity);

            SSS.SetChannel(_loadedProfile?.Encoding?.SSS);

            Emissive.SetChannel(_loadedProfile?.Encoding?.Emissive);
        }

        public void UpdateDefaultValues()
        {
            var encoding = TextureEncoding.GetFactory(_loadedProfile?.Encoding?.Format);
            var encodingDefaults = encoding?.Create();
            var sampler = _loadedProfile?.Encoding?.Sampler ?? Samplers.Nearest;

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
            F0.ApplyDefaultValues(encodingDefaults?.F0, sampler);

            Porosity.ApplyDefaultValues(encodingDefaults?.Porosity, sampler);

            SSS.ApplyDefaultValues(encodingDefaults?.SSS, sampler);

            Emissive.ApplyDefaultValues(encodingDefaults?.Emissive, sampler);
        }

        private void OnPropertyDataChanged(object sender, EventArgs e)
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class ProfilesDesignVM : PublishProfilesVM
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
                Encoding = {
                    Image = "tga",
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
