using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows.Modals
{
    public partial class AboutWindow
    {
        private readonly ILogger<AboutWindow> logger;
        private readonly IServiceProvider provider;


        public AboutWindow(IServiceProvider provider)
        {
            this.provider = provider;
            logger = provider.GetRequiredService<ILogger<AboutWindow>>();

            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);
        }

        private async Task<bool> ShowPatreonNotificationAsync()
        {
            var window = new PatreonNotificationWindow(provider) {Owner = this};

            try {
                return window.ShowDialog() ?? false;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in PatreonNotificationWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return false;
            }
        }

        private async Task<bool> ShowLicenseAgreementAsync()
        {
            var window = new EndUserLicenseAgreementWindow(provider) {Owner = this};

            try {
                return window.ShowDialog() ?? false;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in EndUserLicenseAgreementWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return false;
            }
        }

        private async Task<bool> ShowTermsOfServiceAsync()
        {
            var window = new TermsOfServiceWindow(provider) {Owner = this};

            try {
                return window.ShowDialog() ?? false;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in TermsOfServiceWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return false;
            }
        }

        private async void OnViewPatreonClicked(object sender, RoutedEventArgs e)
        {
            await ShowPatreonNotificationAsync();
        }

        private async void OnViewEulaClicked(object sender, RoutedEventArgs e)
        {
            await ShowLicenseAgreementAsync();
        }

        private async void OnViewTosClicked(object sender, RoutedEventArgs e)
        {
            await ShowTermsOfServiceAsync();
        }
    }
}
