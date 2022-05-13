using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows.Modals
{
    public partial class EndUserLicenseAgreementWindow
    {
        private readonly ILogger<EndUserLicenseAgreementWindow> logger;


        public EndUserLicenseAgreementWindow(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<EndUserLicenseAgreementWindow>>();

            InitializeComponent();

            Model.Initialize(provider);

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Document.Document = RtfHelper.LoadDocument("PixelGraph.UI.Resources.EULA.rtf");

            foreach (var hyperlink in RtfHelper.GetVisuals(Document.Document).OfType<Hyperlink>())
                hyperlink.RequestNavigate += OnDocumentHyperlinkRequestNavigate;
        }

        private async void OnAcceptButtonClick(object sender, RoutedEventArgs e)
        {
            await Model.SetResultAsync(true);
            await Dispatcher.BeginInvoke(() => DialogResult = true);
        }

        private async void OnDeclineButtonClick(object sender, RoutedEventArgs e)
        {
            await Model.SetResultAsync(false);
            await Dispatcher.BeginInvoke(() => DialogResult = false);
        }

        private async void OnDocumentHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try {
                ProcessHelper.Start(e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to open document link!");
                await this.ShowMessageAsync("Error!", $"Failed to open document link! {error.UnfoldMessageString()}");
            }
        }
    }
}
