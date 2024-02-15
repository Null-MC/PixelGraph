using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels;

internal class TermsOfServiceModel : INotifyPropertyChanged
{
    private readonly IAppSettingsManager appSettings;
    private bool _hasNotAccepted;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool HasNotAccepted {
        get => _hasNotAccepted;
        private init {
            _hasNotAccepted = value;
            OnPropertyChanged();
        }
    }


    public TermsOfServiceModel(IAppSettingsManager appSettings)
    {
        this.appSettings = appSettings;

        HasNotAccepted = appSettings.Data.AcceptedTermsOfServiceVersion != AppSettingsDataModel.CurrentTermsVersion;
    }

    public async Task SetResultAsync(bool result)
    {
        appSettings.Data.AcceptedTermsOfServiceVersion = result
            ? AppSettingsDataModel.CurrentTermsVersion : null;

        await appSettings.SaveAsync();
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class TermsOfServiceViewModel : ModelBase
{
    public TermsOfServiceModel? Data {get; private set;}


    public void Initialize(IServiceProvider provider)
    {
        Data = provider.GetRequiredService<TermsOfServiceModel>();
        OnPropertyChanged(nameof(Data));
    }

    //public Task SetResultAsync(bool result)
    //{
    //    return Data.SetResultAsync(result);
    //}
}
