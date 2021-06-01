using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewMaterialWindow
    {
        public NewMaterialWindow(IServiceProvider provider)
        {
            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnCreateButtonClick(object sender, RoutedEventArgs e)
        {
            // TODO

            DialogResult = true;
        }
    }
}
