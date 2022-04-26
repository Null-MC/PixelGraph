using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.UI.Internal;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.Models
{
    public class PublishProfileDisplayRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ResourcePackProfileProperties Profile {get;}

        public bool IsJavaProfile => Common.IO.GameEdition.Is(Profile.Edition, Common.IO.GameEdition.Java);
        public bool IsBedrockProfile => Common.IO.GameEdition.Is(Profile.Edition, Common.IO.GameEdition.Bedrock);

        public string Name {
            get => Profile.Name;
            set {
                Profile.Name = value;
                OnPropertyChanged();
            }
        }

        public string GameEdition {
            get => Profile.Edition;
            set {
                Profile.Edition = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsJavaProfile));
                OnPropertyChanged(nameof(IsBedrockProfile));
            }
        }

        public string PackDescription {
            get => Profile.Description;
            set {
                Profile.Description = value;
                OnPropertyChanged();
            }
        }

        public string PackTags {
            get => Profile.Tags;
            set {
                Profile.Tags = value;
                OnPropertyChanged();
            }
        }

        public int? PackFormat {
            get => Profile.Format;
            set {
                Profile.Format = value;
                OnPropertyChanged();
            }
        }

        public Guid? PackHeaderUuid {
            get => Profile.HeaderUuid;
            set {
                Profile.HeaderUuid = value;
                OnPropertyChanged();
            }
        }

        public Guid? PackModuleUuid {
            get => Profile.ModuleUuid;
            set {
                Profile.ModuleUuid = value;
                OnPropertyChanged();
            }
        }

        public string TextureFormat {
            get => Profile.Encoding?.Format;
            set {
                Profile.Encoding ??= new ResourcePackOutputProperties();
                Profile.Encoding.Format = value;
                OnPropertyChanged();
            }
        }

        public string ImageEncoding {
            get => Profile.Encoding?.Image;
            set {
                Profile.Encoding ??= new ResourcePackOutputProperties();
                Profile.Encoding.Image = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageEncoding));
            }
        }

        public bool EnablePalette {
            get => Profile.Encoding?.EnablePalette ?? false;
            set {
                Profile.Encoding ??= new ResourcePackOutputProperties();
                Profile.Encoding.EnablePalette = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageEncoding));
            }
        }

        public int? PaletteColors {
            get => Profile.Encoding?.PaletteColors;
            set {
                Profile.Encoding ??= new ResourcePackOutputProperties();
                Profile.Encoding.PaletteColors = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaletteColors));
            }
        }

        public string EncodingSampler {
            get => Profile.Encoding?.Sampler;
            set {
                Profile.Encoding ??= new ResourcePackOutputProperties();
                Profile.Encoding.Sampler = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EncodingSampler));
            }
        }

        public int? TextureSize {
            get => Profile.TextureSize;
            set {
                Profile.TextureSize = value;
                OnPropertyChanged();
            }
        }

        public int? BlockTextureSize {
            get => Profile.BlockTextureSize;
            set {
                Profile.BlockTextureSize = value;
                OnPropertyChanged();
            }
        }

        public int? ItemTextureSize {
            get => Profile.ItemTextureSize;
            set {
                Profile.ItemTextureSize = value;
                OnPropertyChanged();
            }
        }

        public decimal? TextureScale {
            get => Profile.TextureScale;
            set {
                Profile.TextureScale = value;
                OnPropertyChanged();
            }
        }

        public decimal? OcclusionQuality {
            get => Profile.OcclusionQuality;
            set {
                Profile.OcclusionQuality = value;
                OnPropertyChanged();
            }
        }

        public decimal? OcclusionPower {
            get => Profile.OcclusionPower;
            set {
                Profile.OcclusionPower = value;
                OnPropertyChanged();
            }
        }

        public bool? AutoLevelHeight {
            get => Profile.AutoLevelHeight ?? ResourcePackProfileProperties.AutoLevelHeightDefault;
            set {
                Profile.AutoLevelHeight = value;
                OnPropertyChanged();
            }
        }

        public bool? AutoGenerateNormal {
            get => Profile.AutoGenerateNormal ?? ResourcePackProfileProperties.AutoGenerateNormalDefault;
            set {
                Profile.AutoGenerateNormal = value;
                OnPropertyChanged();
            }
        }

        public bool? AutoGenerateOcclusion {
            get => Profile.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;
            set {
                Profile.AutoGenerateOcclusion = value;
                OnPropertyChanged();
            }
        }

        public bool? BakeOcclusionToColor {
            get => Profile.BakeOcclusionToColor ?? ResourcePackProfileProperties.BakeOcclusionToColorDefault;
            set {
                Profile.BakeOcclusionToColor = value;
                OnPropertyChanged();
            }
        }


        public PublishProfileDisplayRow(ResourcePackProfileProperties profile)
        {
            Profile = profile;
        }

        //public void InvalidateValues()
        //{
        //    OnPropertyChanged(nameof(Name));
        //    OnPropertyChanged(nameof(GameEdition));
        //    OnPropertyChanged(nameof(PackDescription));
        //    OnPropertyChanged(nameof(PackTags));
        //    OnPropertyChanged(nameof(PackFormat));
        //    OnPropertyChanged(nameof(PackHeaderUuid));
        //    OnPropertyChanged(nameof(PackModuleUuid));
        //    OnPropertyChanged(nameof(TextureFormat));
        //    OnPropertyChanged(nameof(ImageEncoding));
        //    OnPropertyChanged(nameof(EnablePalette));
        //    OnPropertyChanged(nameof(PaletteColors));
        //    OnPropertyChanged(nameof(EncodingSampler));
        //    OnPropertyChanged(nameof(TextureSize));
        //    OnPropertyChanged(nameof(BlockTextureSize));
        //    OnPropertyChanged(nameof(ItemTextureSize));
        //    OnPropertyChanged(nameof(TextureScale));
        //    OnPropertyChanged(nameof(OcclusionQuality));
        //    OnPropertyChanged(nameof(OcclusionPower));
        //    OnPropertyChanged(nameof(AutoGenerateNormal));
        //    OnPropertyChanged(nameof(AutoGenerateOcclusion));
        //    OnPropertyChanged(nameof(BakeOcclusionToColor));

        //    OnPropertyChanged(nameof(IsBedrockProfile));
        //    OnPropertyChanged(nameof(IsJavaProfile));
        //}

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class PublishProfilesModel : ModelBase
    {
        protected ObservableCollection<PublishProfileDisplayRow> _profiles;
        private PublishProfileDisplayRow _selectedProfile;

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
        public TextureChannelMapping HCM {get; set;}
        public TextureChannelMapping F0 {get; set;}

        public TextureChannelMapping Porosity {get; set;}

        public TextureChannelMapping SSS {get; set;}

        public TextureChannelMapping Emissive {get; set;}

        public bool HasSelectedProfile => _selectedProfile != null;
        public bool IsSelectedProfileJava => SelectedProfile?.IsJavaProfile ?? false;
        public bool IsSelectedProfileBedrock => SelectedProfile?.IsBedrockProfile ?? false;
        public decimal OcclusionQualityDefault => ResourcePackProfileProperties.DefaultOcclusionQuality;
        public decimal OcclusionPowerDefault => ResourcePackProfileProperties.DefaultOcclusionPower;

        public ObservableCollection<PublishProfileDisplayRow> Profiles {
            get => _profiles;
            set {
                _profiles = value;
                OnPropertyChanged();

                InvalidateValues();
            }
        }

        public PublishProfileDisplayRow SelectedProfile {
            get => _selectedProfile;
            set {
                _selectedProfile = value;
                OnPropertyChanged();

                InvalidateValues();
                //UpdateDefaultValues();
            }
        }

        public string EditGameEdition {
            get => _selectedProfile?.GameEdition;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.GameEdition = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsSelectedProfileJava));
                OnPropertyChanged(nameof(IsSelectedProfileBedrock));
                UpdateDefaultValues();
            }
        }

        public string EditTextureFormat {
            get => _selectedProfile?.TextureFormat;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.TextureFormat = value;
                OnPropertyChanged();

                UpdateDefaultValues();
            }
        }

        public string EditImageEncoding {
            get => _selectedProfile?.ImageEncoding ?? ResourcePackOutputProperties.ImageDefault;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.ImageEncoding = value;
                OnPropertyChanged();

                UpdateDefaultValues();
            }
        }

        public bool EditEnablePalette {
            get => _selectedProfile?.EnablePalette ?? false;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.EnablePalette = value;
                OnPropertyChanged();

                UpdateDefaultValues();
            }
        }

        public int? EditPaletteColors {
            get => _selectedProfile?.PaletteColors ?? ResourcePackOutputProperties.DefaultPaletteColors;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.PaletteColors = value;
                OnPropertyChanged();

                UpdateDefaultValues();
            }
        }

        public string EditEncodingSampler {
            get => _selectedProfile?.EncodingSampler ?? Samplers.Nearest;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.EncodingSampler = value;
                OnPropertyChanged();

                UpdateDefaultValues();
            }
        }


        public PublishProfilesModel()
        {
            _profiles = new ObservableCollection<PublishProfileDisplayRow>();

            Opacity = new TextureChannelMapping("Opacity");

            ColorRed = new TextureChannelMapping("Color Red");
            ColorGreen = new TextureChannelMapping("Color Green");
            ColorBlue = new TextureChannelMapping("Color Blue");

            Height = new TextureChannelMapping("Height");
            Occlusion = new TextureChannelMapping("Occlusion");

            NormalX = new TextureChannelMapping("Normal-X");
            NormalY = new TextureChannelMapping("Normal-Y");
            NormalZ = new TextureChannelMapping("Normal-Z");

            Specular = new TextureChannelMapping("Specular");

            Smooth = new TextureChannelMapping("Smooth");
            Rough = new TextureChannelMapping("Rough");

            Metal = new TextureChannelMapping("Metal");
            HCM = new TextureChannelMapping("HCM");
            F0 = new TextureChannelMapping("F0");

            Porosity = new TextureChannelMapping("Porosity");

            SSS = new TextureChannelMapping("SubSurface Scattering");

            Emissive = new TextureChannelMapping("Emissive");


            //Opacity.DataChanged += OnPropertyDataChanged;
            
            //ColorRed.DataChanged += OnPropertyDataChanged;
            //ColorGreen.DataChanged += OnPropertyDataChanged;
            //ColorBlue.DataChanged += OnPropertyDataChanged;

            //Height.DataChanged += OnPropertyDataChanged;
            //Occlusion.DataChanged += OnPropertyDataChanged;

            //NormalX.DataChanged += OnPropertyDataChanged;
            //NormalY.DataChanged += OnPropertyDataChanged;
            //NormalZ.DataChanged += OnPropertyDataChanged;

            //Specular.DataChanged += OnPropertyDataChanged;

            //Smooth.DataChanged += OnPropertyDataChanged;
            //Rough.DataChanged += OnPropertyDataChanged;

            //Metal.DataChanged += OnPropertyDataChanged;
            //HCM.DataChanged += OnPropertyDataChanged;
            //F0.DataChanged += OnPropertyDataChanged;

            //Porosity.DataChanged += OnPropertyDataChanged;

            //SSS.DataChanged += OnPropertyDataChanged;

            //Emissive.DataChanged += OnPropertyDataChanged;
        }

        private void InvalidateValues()
        {
            OnPropertyChanged(nameof(HasSelectedProfile));
            OnPropertyChanged(nameof(IsSelectedProfileJava));
            OnPropertyChanged(nameof(IsSelectedProfileBedrock));

            OnPropertyChanged(nameof(EditGameEdition));
            OnPropertyChanged(nameof(EditTextureFormat));
            OnPropertyChanged(nameof(EditImageEncoding));
            OnPropertyChanged(nameof(EditPaletteColors));
            OnPropertyChanged(nameof(EditEncodingSampler));

            //_selectedProfile?.InvalidateValues();

            UpdateChannels();
            UpdateDefaultValues();
        }

        private void UpdateChannels()
        {
            Opacity.SetChannel(_selectedProfile?.Profile.Encoding?.Opacity);

            ColorRed.SetChannel(_selectedProfile?.Profile.Encoding?.ColorRed);
            ColorGreen.SetChannel(_selectedProfile?.Profile.Encoding?.ColorGreen);
            ColorBlue.SetChannel(_selectedProfile?.Profile.Encoding?.ColorBlue);

            Height.SetChannel(_selectedProfile?.Profile.Encoding?.Height);
            Occlusion.SetChannel(_selectedProfile?.Profile.Encoding?.Occlusion);

            NormalX.SetChannel(_selectedProfile?.Profile.Encoding?.NormalX);
            NormalY.SetChannel(_selectedProfile?.Profile.Encoding?.NormalY);
            NormalZ.SetChannel(_selectedProfile?.Profile.Encoding?.NormalZ);

            Specular.SetChannel(_selectedProfile?.Profile.Encoding?.Specular);

            Smooth.SetChannel(_selectedProfile?.Profile.Encoding?.Smooth);
            Rough.SetChannel(_selectedProfile?.Profile.Encoding?.Rough);

            Metal.SetChannel(_selectedProfile?.Profile.Encoding?.Metal);
            HCM.SetChannel(_selectedProfile?.Profile.Encoding?.HCM);
            F0.SetChannel(_selectedProfile?.Profile.Encoding?.F0);

            Porosity.SetChannel(_selectedProfile?.Profile.Encoding?.Porosity);

            SSS.SetChannel(_selectedProfile?.Profile.Encoding?.SSS);

            Emissive.SetChannel(_selectedProfile?.Profile.Encoding?.Emissive);
        }

        public void UpdateDefaultValues()
        {
            var encoding = Common.TextureFormats.TextureFormat.GetFactory(_selectedProfile?.Profile.Encoding?.Format);
            var encodingDefaults = encoding?.Create();
            var sampler = _selectedProfile?.Profile.Encoding?.Sampler ?? Samplers.Nearest;

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
            HCM.ApplyDefaultValues(encodingDefaults?.HCM, sampler);
            F0.ApplyDefaultValues(encodingDefaults?.F0, sampler);

            Porosity.ApplyDefaultValues(encodingDefaults?.Porosity, sampler);

            SSS.ApplyDefaultValues(encodingDefaults?.SSS, sampler);

            Emissive.ApplyDefaultValues(encodingDefaults?.Emissive, sampler);
        }
    }

    internal class PublishProfilesDesignerModel : PublishProfilesModel
    {
        public PublishProfilesDesignerModel()
        {
            _profiles.Add(new PublishProfileDisplayRow(new ResourcePackProfileProperties {
                Edition = GameEdition.Bedrock,
                Name = "Sample RP",
                Description = "A description of the resource pack.",
                Format = 7,
                Encoding = {
                    Image = ImageExtensions.Jpg,
                    Sampler = Samplers.Nearest,
                },
            }));

            _profiles.Add(new PublishProfileDisplayRow(new ResourcePackProfileProperties {Name = "Profile B"}));
            _profiles.Add(new PublishProfileDisplayRow(new ResourcePackProfileProperties {Name = "Profile C"}));

            SelectedProfile = _profiles[0];
        }
    }
}
