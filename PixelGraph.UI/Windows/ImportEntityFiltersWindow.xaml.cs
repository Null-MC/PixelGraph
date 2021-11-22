using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class ImportEntityFiltersWindow
    {
        private readonly ImportEntityFiltersViewModel viewModel;


        public ImportEntityFiltersWindow(IServiceProvider provider)
        {
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new ImportEntityFiltersViewModel {
                Model = Model,
            };

            viewModel.UpdateEntityList();
            //viewModel.UpdateLocation();
        }

        private void OnGameVersionChanged(object sender, EventArgs e)
        {
            viewModel.UpdateEntityList();
            //viewModel.UpdateLocation();
        }

        //private void OnGameEntityNameChanged(object sender, EventArgs e)
        //{
        //    viewModel.UpdateLocation();
        //}

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
