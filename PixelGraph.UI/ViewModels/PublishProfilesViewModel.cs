using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Projects;
using PixelGraph.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels;

internal class PublishProfilesModel : INotifyPropertyChanged
{
    private readonly IProjectContextManager? projectContextMgr;
    private string? _defaultPackName, _defaultPackDescription;
    private PublishProfileEditModel? _selectedProfile;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool HasSelectedProfile => _selectedProfile != null;
    public bool IsSelectedProfileJava => SelectedProfile?.IsJavaProfile ?? false;
    public bool IsSelectedProfileBedrock => SelectedProfile?.IsBedrockProfile ?? false;
    public string NormalMethodDefault => PublishProfileProperties.DefaultNormalMethod;
    public decimal NormalStrengthDefault => PublishProfileProperties.DefaultNormalStrength;
    public decimal OcclusionQualityDefault => PublishProfileProperties.DefaultOcclusionQuality;
    public decimal OcclusionPowerDefault => PublishProfileProperties.DefaultOcclusionPower;

    public ObservableCollection<PublishProfileEditModel> Profiles {get;}

    public PublishProfileEditModel? SelectedProfile {
        get => _selectedProfile;
        set {
            _selectedProfile = value;
            OnPropertyChanged();

            InvalidateValues();
        }
    }

    public string? DefaultPackName {
        get => _defaultPackName;
        private set {
            _defaultPackName = value;
            OnPropertyChanged();
        }
    }

    public string? DefaultPackDescription {
        get => _defaultPackDescription;
        private set {
            _defaultPackDescription = value;
            OnPropertyChanged();
        }
    }

    public string? EditGameEdition {
        get => _selectedProfile?.GameEdition;
        set {
            if (_selectedProfile == null) return;
            _selectedProfile.GameEdition = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(IsSelectedProfileJava));
            OnPropertyChanged(nameof(IsSelectedProfileBedrock));
        }
    }

    public string? EditTextureFormat {
        get => _selectedProfile?.TextureFormat;
        set {
            if (_selectedProfile == null) return;
            _selectedProfile.TextureFormat = value;
            OnPropertyChanged();
        }
    }

    public string? EditImageEncoding {
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

    public string? EditEncodingSampler {
        get => _selectedProfile?.EncodingSampler ?? Samplers.Nearest;
        set {
            if (_selectedProfile == null) return;
            _selectedProfile.EncodingSampler = value;
            OnPropertyChanged();
        }
    }


    public PublishProfilesModel(IProjectContextManager projectContextMgr)
    {
        Profiles = new ObservableCollection<PublishProfileEditModel>();

        this.projectContextMgr = projectContextMgr;

        var projectContext = projectContextMgr.GetContextRequired();
        if (projectContext.Project == null) throw new ApplicationException("Project context is undefined!");

        DefaultPackName = projectContext.Project.Name;
        DefaultPackDescription = projectContext.Project.Description;

        if (projectContext.Project.Profiles != null) {
            foreach (var profile in projectContext.Project.Profiles)
                Profiles.Add(new PublishProfileEditModel(profile) {
                    DefaultName = DefaultPackName,
                });
        }

        if (projectContext.SelectedProfile != null)
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == projectContext.SelectedProfile.Name);
    }

    public void CreateNewProfile()
    {
        ArgumentNullException.ThrowIfNull(projectContextMgr);

        var context = projectContextMgr.GetContextRequired();
        if (context.Project == null) throw new ApplicationException("Project context is undefined!");

        var profile = new PublishProfileProperties {
            Name = context.Project.Name ?? "New Profile",
            Encoding = new PackOutputEncoding {
                Format = TextureFormat.Format_Lab13,
            },
        };

        AddRow(profile);
    }

    public void CloneSelectedProfile()
    {
        var profile = SelectedProfile?.Profile;
        if (profile != null) AddRow((PublishProfileProperties)profile.Clone());
    }

    public void RemoveSelected()
    {
        var profile = SelectedProfile;
        if (profile != null) Profiles.Remove(profile);
    }

    public async Task SaveAsync()
    {
        ArgumentNullException.ThrowIfNull(projectContextMgr);

        var context = projectContextMgr.GetContextRequired();
        if (context.Project == null) throw new ApplicationException("Project context is undefined!");

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
        OnPropertyChanged(nameof(EditEnablePalette));
        OnPropertyChanged(nameof(EditPaletteColors));
        OnPropertyChanged(nameof(EditEncodingSampler));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

internal class PublishProfilesViewModel : ModelBase
{
    public PublishProfilesModel? Data {get; private set;}


    public void Initialize(IServiceProvider provider)
    {
        Data = provider.GetRequiredService<PublishProfilesModel>();
        OnPropertyChanged(nameof(Data));
    }

    internal class DesignerModel : PublishProfilesViewModel
    {
        protected DesignerModel()
        {
            Data = new PublishProfilesModel(null);
        }
    }
}

internal class PublishProfilesDesignerModel : PublishProfilesViewModel.DesignerModel
{
    public PublishProfilesDesignerModel()
    {
        Data.Profiles.Add(new PublishProfileEditModel(new PublishProfileProperties {
            Edition = GameEdition.Bedrock,
            Name = "Sample RP",
            Description = "A description of the resource pack.",
            Format = 7,
            Encoding = new PackOutputEncoding {
                Image = ImageExtensions.Jpg,
                Sampler = Samplers.Nearest,
            },
        }));

        Data.Profiles.Add(new PublishProfileEditModel(new PublishProfileProperties {Name = "Profile B"}));
        Data.Profiles.Add(new PublishProfileEditModel(new PublishProfileProperties {Name = "Profile C"}));

        Data.SelectedProfile = Data.Profiles[0];
    }
}
