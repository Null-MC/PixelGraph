using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels;

internal class EndUserLicenseAgreementViewModel : ModelBase
{
    private IAppSettingsManager appSettings;
    private bool _hasNotAccepted;

    public bool HasNotAccepted {
        get => _hasNotAccepted;
        private set {
            _hasNotAccepted = value;
            OnPropertyChanged();
        }
    }


    public void Initialize(IServiceProvider provider)
    {
        appSettings = provider.GetRequiredService<IAppSettingsManager>();
        HasNotAccepted = appSettings.Data.AcceptedLicenseAgreementVersion != AppSettingsDataModel.CurrentLicenseVersion;
    }

    public async Task SetResultAsync(bool result)
    {
        appSettings.Data.AcceptedLicenseAgreementVersion = result
            ? AppSettingsDataModel.CurrentLicenseVersion : null;

        await appSettings.SaveAsync();
    }
}