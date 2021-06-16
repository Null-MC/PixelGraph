using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewMaterialWindow
    {
        private readonly NewMaterialViewModel viewModel;


        public NewMaterialWindow(IServiceProvider provider)
        {
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new NewMaterialViewModel {
                Model = Model,
            };

            viewModel.UpdateBlockList();
            viewModel.UpdateLocation();
        }

        private void OnGameObjectTypeChanged(object sender, EventArgs e)
        {
            viewModel.UpdateBlockList();
            viewModel.UpdateLocation();
        }

        private void OnGameObjectNameChanged(object sender, EventArgs e)
        {
            viewModel.UpdateLocation();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnCreateButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
