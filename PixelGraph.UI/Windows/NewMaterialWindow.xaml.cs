using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewMaterialWindow
    {
        private readonly IServiceProvider provider;


        public NewMaterialWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);

            Model.UpdateBlockList();
            Model.UpdateLocation();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Model.Initialize(provider);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void OnCreateButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Model.GameNamespace)) {
                NamespaceComboBox.Focus();
                //MessageBox.Show(this, "Namespace cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await this.ShowMessageAsync("Error", "Namespace cannot be empty!");
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.GameObjectName)) {
                NameComboBox.Focus();
                //MessageBox.Show(this, "Name cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await this.ShowMessageAsync("Error", "Name cannot be empty!");
                return;
            }

            DialogResult = true;
        }
    }
}
