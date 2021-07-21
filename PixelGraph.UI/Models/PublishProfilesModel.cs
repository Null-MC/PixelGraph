using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewData;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace PixelGraph.UI.Models
{
    internal class PublishProfilesModel : ModelBase
    {
        protected ObservableCollection<ProfileItem> _profiles;
        private ResourcePackProfileProperties _loadedProfile;
        private ProfileItem _selectedProfileItem;
        private string _rootDirectory;

        public event EventHandler DataChanged;

        public TextureChannelMapping[] EncodingChannels {get;}

        public TextureChannelMapping Opacity {get;}

        public TextureChannelMapping ColorRed {get;}
        public TextureChannelMapping ColorGreen {get;}
        public TextureChannelMapping ColorBlue {get;}

        public TextureChannelMapping Height {get; set;}

        public TextureChannelMapping Occlusion {get; set;}

        public TextureChannelMapping NormalX {get; set;}
        public TextureChannelMapping NormalY {get; set;}
        public TextureChannelMapping NormalZ {get; set;}

        public TextureChannelMapping Specular {get; set;}

        public TextureChannelMapping Smooth {get; set;}
        public TextureChannelMapping Rough {get; set;}

        public TextureChannelMapping Metal {get; set;}
        public TextureChannelMapping F0 {get; set;}

        public TextureChannelMapping Porosity {get; set;}

        public TextureChannelMapping SSS {get; set;}

        public TextureChannelMapping Emissive {get; set;}

        public bool HasSelectedProfile => _selectedProfileItem != null;
        public bool HasLoadedProfile => _loadedProfile != null;
        public bool IsJavaProfile => GameEditions.Is(_loadedProfile?.Edition, GameEditions.Java);
        public bool IsBedrockProfile => GameEditions.Is(_loadedProfile?.Edition, GameEditions.Bedrock);
        public decimal OcclusionQualityDefault => ResourcePackProfileProperties.DefaultOcclusionQuality;
        public decimal OcclusionPowerDefault => ResourcePackProfileProperties.DefaultOcclusionPower;
        public string PackName => _loadedProfile?.Name;
        public string EncodingSampler => _loadedProfile?.Encoding?.Sampler;
        public string ImageEncoding => _loadedProfile?.Encoding?.Image;

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

                InvalidateValues();
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

        public string EditPackName {
            get => _loadedProfile?.Name ?? GetDefaultPackName();
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PackName));

                OnDataChanged();
            }
        }

        public string GameEdition {
            get => _loadedProfile?.Edition;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Edition = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsJavaProfile));
                OnPropertyChanged(nameof(IsBedrockProfile));
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

        public int? PackFormat {
            get => _loadedProfile?.Format;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Format = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public Guid? PackHeaderUuid {
            get => _loadedProfile?.HeaderUuid;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.HeaderUuid = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public Guid? PackModuleUuid {
            get => _loadedProfile?.ModuleUuid;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.ModuleUuid = value;
                OnPropertyChanged();

                OnDataChanged();
            }
        }

        public string TextureFormat {
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

        public string EditImageEncoding {
            get => _loadedProfile?.Encoding?.Image ?? ResourcePackOutputProperties.ImageDefault;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Encoding ??= new ResourcePackOutputProperties();
                _loadedProfile.Encoding.Image = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageEncoding));

                UpdateDefaultValues();
                OnDataChanged();
            }
        }

        public string EditEncodingSampler {
            get => _loadedProfile?.Encoding?.Sampler ?? Samplers.Nearest;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Encoding ??= new ResourcePackOutputProperties();
                _loadedProfile.Encoding.Sampler = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EncodingSampler));

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

        public decimal? OcclusionQuality {
            get => _loadedProfile?.OcclusionQuality;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.OcclusionQuality = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? OcclusionPower {
            get => _loadedProfile?.OcclusionPower;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.OcclusionPower = value;
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

        public bool? BakeOcclusionToColor {
            get => _loadedProfile?.BakeOcclusionToColor ?? ResourcePackProfileProperties.BakeOcclusionToColorDefault;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.BakeOcclusionToColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        public PublishProfilesModel()
        {
            _profiles = new ObservableCollection<ProfileItem>();

            EncodingChannels = new []{
                Opacity = new TextureChannelMapping("Opacity"),

                ColorRed = new TextureChannelMapping("Color Red"),
                ColorGreen = new TextureChannelMapping("Color Green"),
                ColorBlue = new TextureChannelMapping("Color Blue"),

                Height = new TextureChannelMapping("Height"),
                Occlusion = new TextureChannelMapping("Occlusion"),

                NormalX = new TextureChannelMapping("Normal-X"),
                NormalY = new TextureChannelMapping("Normal-Y"),
                NormalZ = new TextureChannelMapping("Normal-Z"),

                Specular = new TextureChannelMapping("Specular"),

                Smooth = new TextureChannelMapping("Smooth"),
                Rough = new TextureChannelMapping("Rough"),

                Metal = new TextureChannelMapping("Metal"),
                F0 = new TextureChannelMapping("F0"),

                Porosity = new TextureChannelMapping("Porosity"),

                SSS = new TextureChannelMapping("SubSurface Scattering"),

                Emissive = new TextureChannelMapping("Emissive"),
            };

            Opacity.DataChanged += OnPropertyDataChanged;
            
            ColorRed.DataChanged += OnPropertyDataChanged;
            ColorGreen.DataChanged += OnPropertyDataChanged;
            ColorBlue.DataChanged += OnPropertyDataChanged;

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

        private void InvalidateValues()
        {
            OnPropertyChanged(nameof(IsBedrockProfile));
            OnPropertyChanged(nameof(IsJavaProfile));
            OnPropertyChanged(nameof(HasLoadedProfile));
            OnPropertyChanged(nameof(PackName));
            OnPropertyChanged(nameof(EditPackName));
            OnPropertyChanged(nameof(GameEdition));
            OnPropertyChanged(nameof(PackDescription));
            OnPropertyChanged(nameof(PackTags));
            OnPropertyChanged(nameof(PackFormat));
            OnPropertyChanged(nameof(PackHeaderUuid));
            OnPropertyChanged(nameof(PackModuleUuid));
            OnPropertyChanged(nameof(TextureFormat));
            OnPropertyChanged(nameof(ImageEncoding));
            OnPropertyChanged(nameof(EditImageEncoding));
            OnPropertyChanged(nameof(EncodingSampler));
            OnPropertyChanged(nameof(EditEncodingSampler));
            OnPropertyChanged(nameof(TextureSize));
            OnPropertyChanged(nameof(BlockTextureSize));
            OnPropertyChanged(nameof(TextureScale));
            OnPropertyChanged(nameof(OcclusionQuality));
            OnPropertyChanged(nameof(OcclusionPower));
            OnPropertyChanged(nameof(AutoGenerateNormal));
            OnPropertyChanged(nameof(AutoGenerateOcclusion));
            OnPropertyChanged(nameof(BakeOcclusionToColor));

            UpdateChannels();
            UpdateDefaultValues();
        }

        private void UpdateChannels()
        {
            Opacity.SetChannel(_loadedProfile?.Encoding?.Opacity);

            ColorRed.SetChannel(_loadedProfile?.Encoding?.ColorRed);
            ColorGreen.SetChannel(_loadedProfile?.Encoding?.ColorGreen);
            ColorBlue.SetChannel(_loadedProfile?.Encoding?.ColorBlue);

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
            var encoding = Common.TextureFormats.TextureFormat.GetFactory(_loadedProfile?.Encoding?.Format);
            var encodingDefaults = encoding?.Create();
            var sampler = _loadedProfile?.Encoding?.Sampler ?? Samplers.Nearest;

            Opacity.ApplyDefaultValues(encodingDefaults?.Opacity, sampler);

            ColorRed.ApplyDefaultValues(encodingDefaults?.ColorRed, sampler);
            ColorGreen.ApplyDefaultValues(encodingDefaults?.ColorGreen, sampler);
            ColorBlue.ApplyDefaultValues(encodingDefaults?.ColorBlue, sampler);

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

        private string GetDefaultPackName()
        {
            var name = _loadedProfile?.LocalFile;
            if (string.IsNullOrWhiteSpace(name)) return null;

            name = Path.GetFileName(name);
            if (string.IsNullOrWhiteSpace(name)) return null;

            if (name.EndsWith(".pack.yml")) name = name[..^9];
            return string.IsNullOrWhiteSpace(name) ? null : name;
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

    internal class PublishProfilesDesignerModel : PublishProfilesModel
    {
        public PublishProfilesDesignerModel()
        {
            _profiles.Add(new ProfileItem {Name = "Profile A"});
            _profiles.Add(new ProfileItem {Name = "Profile B"});
            _profiles.Add(new ProfileItem {Name = "Profile C"});

            SelectedProfileItem = _profiles[0];

            LoadedProfile = new ResourcePackProfileProperties {
                Edition = GameEditions.Bedrock,
                Name = "Sample RP",
                Description = "A description of the resource pack.",
                Format = 7,
                Encoding = {
                    Image = ImageExtensions.Jpg,
                    Sampler = Samplers.Nearest,
                },
            };
        }
    }
}
