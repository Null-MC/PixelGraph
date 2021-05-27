using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.ViewData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        public IEnumerable<TextureFormatValueItem> TextureFormatOptions => GetTextureFormatOptions(GameEdition);

        public bool HasSelectedProfile => _selectedProfileItem != null;
        public bool HasLoadedProfile => _loadedProfile != null;
        public bool IsJavaProfile => GameEditions.Is(_loadedProfile?.Edition, GameEditions.Java);
        public bool IsBedrockProfile => GameEditions.Is(_loadedProfile?.Edition, GameEditions.Bedrock);
        public decimal OcclusionQualityDefault => ResourcePackProfileProperties.DefaultOcclusionQuality;
        public decimal OcclusionPowerDefault => ResourcePackProfileProperties.DefaultOcclusionPower;

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
                
                OnPropertyChanged(nameof(TextureFormatOptions));
                OnPropertyChanged(nameof(IsBedrockProfile));
                OnPropertyChanged(nameof(IsJavaProfile));
                OnPropertyChanged(nameof(HasLoadedProfile));
                OnPropertyChanged(nameof(PackName));
                OnPropertyChanged(nameof(GameEdition));
                OnPropertyChanged(nameof(PackDescription));
                OnPropertyChanged(nameof(PackTags));
                OnPropertyChanged(nameof(PackFormat));
                OnPropertyChanged(nameof(PackHeaderUuid));
                OnPropertyChanged(nameof(PackModuleUuid));
                OnPropertyChanged(nameof(TextureFormat));
                OnPropertyChanged(nameof(EncodingSampler));
                OnPropertyChanged(nameof(TextureSize));
                OnPropertyChanged(nameof(BlockTextureSize));
                OnPropertyChanged(nameof(TextureScale));
                OnPropertyChanged(nameof(OcclusionQuality));
                OnPropertyChanged(nameof(OcclusionPower));
                OnPropertyChanged(nameof(AutoGenerateNormal));
                OnPropertyChanged(nameof(AutoGenerateOcclusion));

                UpdateChannels();
                //UpdateTextureFormatOptions();
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

        public string PackName {
            get => _loadedProfile?.Name;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Name = value;
                OnPropertyChanged();

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
                OnPropertyChanged(nameof(TextureFormatOptions));
                //UpdateTextureFormatOptions();
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

        public string ImageEncoding {
            get => _loadedProfile?.Encoding?.Image;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Encoding ??= new ResourcePackOutputProperties();
                _loadedProfile.Encoding.Image = value;
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

            //TextureFormatOptions = new ObservableCollection<TextureFormatValueItem>();

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

        //private void UpdateTextureFormatOptions()
        //{
        //    isTextureFormatOptionsUpdating = true;

        //    TextureFormatOptions.Clear();

        //    TextureFormatOptions.Add(new TextureFormatValueItem {Text = "Raw", Value = TextureEncoding.Format_Raw, Hint = RawFormat.Description});
        //    TextureFormatOptions.Add(new TextureFormatValueItem {Text = "Diffuse", Value = TextureEncoding.Format_Diffuse, Hint = DiffuseFormat.Description});

        //    if (GameEditions.Is(GameEdition, GameEditions.Java)) {
        //        TextureFormatOptions.Add(new TextureFormatValueItem {Text = "Specular", Value = TextureEncoding.Format_Specular, Hint = SpecularFormat.Description});
        //        TextureFormatOptions.Add(new TextureFormatValueItem {Text = "OldPbr", Value = TextureEncoding.Format_OldPbr, Hint = OldPbrFormat.Description});
        //        TextureFormatOptions.Add(new TextureFormatValueItem {Text = "LabPbr 1.1", Value = TextureEncoding.Format_Lab11, Hint = LabPbr11Format.Description});
        //        TextureFormatOptions.Add(new TextureFormatValueItem {Text = "LabPbr 1.2", Value = TextureEncoding.Format_Lab12, Hint = LabPbr12Format.Description});
        //        TextureFormatOptions.Add(new TextureFormatValueItem {Text = "LabPbr 1.3", Value = TextureEncoding.Format_Lab13, Hint = LabPbr13Format.Description});
        //    }

        //    if (GameEditions.Is(GameEdition, GameEditions.Bedrock)) {
        //        TextureFormatOptions.Add(new TextureFormatValueItem {Text = "RTX", Value = TextureEncoding.Format_Rtx, Hint = RtxFormat.Description});
        //    }

        //    isTextureFormatOptionsUpdating = false;
        //}

        private void OnPropertyDataChanged(object sender, EventArgs e)
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        //protected virtual void OnCollectionChanged([CallerMemberName] string propertyName = null)
        //{
        //    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, propertyName));
        //}

        private static IEnumerable<TextureFormatValueItem> GetTextureFormatOptions(string edition) =>
            new AllTextureFormatValues().Where(i => i.GameEditions.Contains(edition));
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
