using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class TermsOfServiceViewModel : ModelBase
    {
        private IAppSettings appSettings;
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
            appSettings = provider.GetRequiredService<IAppSettings>();
            HasNotAccepted = appSettings.Data.AcceptedTermsOfServiceVersion != AppSettingsDataModel.CurrentTermsVersion;
        }

        public async Task SetResultAsync(bool result)
        {
            appSettings.Data.AcceptedTermsOfServiceVersion = result
                ? AppSettingsDataModel.CurrentTermsVersion : null;

            await appSettings.SaveAsync();
        }
    }
}
