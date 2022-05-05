using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Extensions;
using System;

namespace PixelGraph.UI.Models
{
    public class PublishProfileEditModel : ModelBase
    {
        public PublishProfileProperties Profile {get;}
        private string _defaultName;

        public bool IsJavaProfile => Common.IO.GameEdition.Is(Profile.Edition, Common.IO.GameEdition.Java);
        public bool IsBedrockProfile => Common.IO.GameEdition.Is(Profile.Edition, Common.IO.GameEdition.Bedrock);
        public string DisplayName => Profile.Name?.NullIfWhitespace(_defaultName);

        public string Name {
            get => Profile.Name;
            set {
                Profile.Name = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string DefaultName {
            //get => _defaultName;
            set {
                _defaultName = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(DisplayName));
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
                Profile.Encoding ??= new PackOutputEncoding();
                Profile.Encoding.Format = value;
                OnPropertyChanged();
            }
        }

        public string ImageEncoding {
            get => Profile.Encoding?.Image;
            set {
                Profile.Encoding ??= new PackOutputEncoding();
                Profile.Encoding.Image = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageEncoding));
            }
        }

        public bool EnablePalette {
            get => Profile.Encoding?.EnablePalette ?? false;
            set {
                Profile.Encoding ??= new PackOutputEncoding();
                Profile.Encoding.EnablePalette = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageEncoding));
            }
        }

        public int? PaletteColors {
            get => Profile.Encoding?.PaletteColors;
            set {
                Profile.Encoding ??= new PackOutputEncoding();
                Profile.Encoding.PaletteColors = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaletteColors));
            }
        }

        public string EncodingSampler {
            get => Profile.Encoding?.Sampler;
            set {
                Profile.Encoding ??= new PackOutputEncoding();
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
            get => Profile.AutoLevelHeight ?? PublishProfileProperties.AutoLevelHeightDefault;
            set {
                Profile.AutoLevelHeight = value;
                OnPropertyChanged();
            }
        }

        public bool? AutoGenerateNormal {
            get => Profile.AutoGenerateNormal ?? PublishProfileProperties.AutoGenerateNormalDefault;
            set {
                Profile.AutoGenerateNormal = value;
                OnPropertyChanged();
            }
        }

        public bool? AutoGenerateOcclusion {
            get => Profile.AutoGenerateOcclusion ?? PublishProfileProperties.AutoGenerateOcclusionDefault;
            set {
                Profile.AutoGenerateOcclusion = value;
                OnPropertyChanged();
            }
        }

        public bool? BakeOcclusionToColor {
            get => Profile.BakeOcclusionToColor ?? PublishProfileProperties.BakeOcclusionToColorDefault;
            set {
                Profile.BakeOcclusionToColor = value;
                OnPropertyChanged();
            }
        }


        public PublishProfileEditModel(PublishProfileProperties profile)
        {
            Profile = profile;
        }
    }
}
