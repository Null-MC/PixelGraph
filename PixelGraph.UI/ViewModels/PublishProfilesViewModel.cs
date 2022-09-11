using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Projects;
using PixelGraph.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishProfilesViewModel : ModelBase
    {
        private IProjectContextManager projectContextMgr;
        private PublishProfileEditModel _selectedProfile;
        private string _defaultPackName, _defaultPackDescription;

        public bool HasSelectedProfile => _selectedProfile != null;
        public bool IsSelectedProfileJava => SelectedProfile?.IsJavaProfile ?? false;
        public bool IsSelectedProfileBedrock => SelectedProfile?.IsBedrockProfile ?? false;
        public string NormalMethodDefault => PublishProfileProperties.DefaultNormalMethod;
        public decimal NormalStrengthDefault => PublishProfileProperties.DefaultNormalStrength;
        public decimal OcclusionQualityDefault => PublishProfileProperties.DefaultOcclusionQuality;
        public decimal OcclusionPowerDefault => PublishProfileProperties.DefaultOcclusionPower;

        public ObservableCollection<PublishProfileEditModel> Profiles {get;}

        public PublishProfileEditModel SelectedProfile {
            get => _selectedProfile;
            set {
                _selectedProfile = value;
                OnPropertyChanged();

                InvalidateValues();
            }
        }

        public string DefaultPackName {
            get => _defaultPackName;
            private set {
                _defaultPackName = value;
                OnPropertyChanged();
            }
        }

        public string DefaultPackDescription {
            get => _defaultPackDescription;
            private set {
                _defaultPackDescription = value;
                OnPropertyChanged();
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
            }
        }

        public string EditTextureFormat {
            get => _selectedProfile?.TextureFormat;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.TextureFormat = value;
                OnPropertyChanged();
            }
        }

        public string EditImageEncoding {
            get => _selectedProfile?.ImageEncoding ?? PackOutputEncoding.ImageDefault;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.ImageEncoding = value;
                OnPropertyChanged();
            }
        }

        public bool EditEnablePalette {
            get => _selectedProfile?.EnablePalette ?? false;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.EnablePalette = value;
                OnPropertyChanged();
            }
        }

        public int? EditPaletteColors {
            get => _selectedProfile?.PaletteColors ?? PackOutputEncoding.DefaultPaletteColors;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.PaletteColors = value;
                OnPropertyChanged();
            }
        }

        public string EditEncodingSampler {
            get => _selectedProfile?.EncodingSampler ?? Samplers.Nearest;
            set {
                if (_selectedProfile == null) return;
                _selectedProfile.EncodingSampler = value;
                OnPropertyChanged();
            }
        }


        public PublishProfilesViewModel()
        {
            Profiles = new ObservableCollection<PublishProfileEditModel>();
        }

        public void Initialize(IServiceProvider provider)
        {
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            var projectContext = projectContextMgr.GetContext();

            DefaultPackName = projectContext.Project.Name;
            DefaultPackDescription = projectContext.Project.Description;

            foreach (var profile in projectContext.Project.Profiles)
                Profiles.Add(new PublishProfileEditModel(profile) {
                    DefaultName = DefaultPackName,
                });

            if (projectContext.SelectedProfile != null)
                SelectedProfile = Profiles.FirstOrDefault(p => p.Name == projectContext.SelectedProfile.Name);
        }

        public void CreateNewProfile()
        {
            var projectContext = projectContextMgr.GetContext();

            var profile = new PublishProfileProperties {
                Name = projectContext.Project.Name ?? "New Profile",
                Encoding = {
                    Format = TextureFormat.Format_Lab13,
                },
            };

            AddRow(profile);
        }

        public void CloneSelectedProfile()
        {
            if (!HasSelectedProfile) return;
            var profile = (PublishProfileProperties)SelectedProfile.Profile.Clone();
            AddRow(profile);
        }

        public void RemoveSelected()
        {
            Profiles.Remove(SelectedProfile);
        }

        public async Task SaveAsync()
        {
            var context = projectContextMgr.GetContext();

            context.Project.Profiles = Profiles.Select(p => p.Profile).ToList();
            context.SelectedProfile = SelectedProfile?.Profile;

            await projectContextMgr.SaveAsync();
        }

        private void AddRow(PublishProfileProperties profile)
        {
            var row = new PublishProfileEditModel(profile);
            Profiles.Add(row);
            SelectedProfile = row;
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
        }
    }

    internal class PublishProfilesDesignerModel : PublishProfilesViewModel
    {
        public PublishProfilesDesignerModel()
        {
            Profiles.Add(new PublishProfileEditModel(new PublishProfileProperties {
                Edition = GameEdition.Bedrock,
                Name = "Sample RP",
                Description = "A description of the resource pack.",
                Format = 7,
                Encoding = {
                    Image = ImageExtensions.Jpg,
                    Sampler = Samplers.Nearest,
                },
            }));

            Profiles.Add(new PublishProfileEditModel(new PublishProfileProperties {Name = "Profile B"}));
            Profiles.Add(new PublishProfileEditModel(new PublishProfileProperties {Name = "Profile C"}));

            SelectedProfile = Profiles[0];
        }
    }
}
