using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;

namespace PixelGraph.UI.ViewModels;

internal class PatreonNotificationViewModel : ModelBase
{
    private IAppSettingsManager? appSettings;
    //private bool _hasNotAccepted;

    //public bool HasNotAccepted {
    //    get => _hasNotAccepted;
    //    private set {
    //        _hasNotAccepted = value;
    //        OnPropertyChanged();
    //    }
    //}


    public void Initialize(IServiceProvider provider)
    {
        appSettings = provider.GetRequiredService<IAppSettingsManager>();
        //HasNotAccepted = appSettings.Data.AcceptedLicenseAgreementVersion != AppSettingsDataModel.CurrentLicenseVersion;
    }

    public async Task AcceptAsync()
    {
        if (appSettings == null) {
            // warn
            return;
        }

        appSettings.Data.HasAcceptedPatreonNotification = true;

        await appSettings.SaveAsync();
    }
}
