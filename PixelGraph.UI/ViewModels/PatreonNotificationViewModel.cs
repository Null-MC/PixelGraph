using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels;

internal class PatreonNotificationViewModel : ModelBase
{
    private IAppSettingsManager appSettings;
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
        appSettings.Data.HasAcceptedPatreonNotification = true;

        await appSettings.SaveAsync();
    }
}