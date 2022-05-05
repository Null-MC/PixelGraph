using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class TermsOfServiceWindow
    {
        public TermsOfServiceWindow(IServiceProvider provider)
        {
            InitializeComponent();

            Model.Initialize(provider);

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Document.Document = Model.LoadDocument();
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
    }
}
