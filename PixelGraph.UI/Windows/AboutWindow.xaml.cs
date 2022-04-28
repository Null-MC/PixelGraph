using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class AboutWindow
    {
        public AboutWindow(IServiceProvider provider)
        {
            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Initialize();
        }
    }
}
