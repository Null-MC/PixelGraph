using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class PublishLocationsWindow
    {
        private readonly PublishLocationsViewModel viewModel;


        public PublishLocationsWindow(IServiceProvider provider)
        {
            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            viewModel = new PublishLocationsViewModel(provider);

            InitializeComponent();

            themeHelper.ApplyCurrent(this);

            viewModel.Model = Model;
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        #region Events

        private async void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (DialogResult.HasValue || !Model.HasChanges) return;

            var result = MessageBox.Show(this, "Would you like to save your changes?", "Warning!", MessageBoxButton.YesNoCancel);

            switch (result) {
                case MessageBoxResult.Yes:
                    await viewModel.SaveChangesAsync();
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void OnPathBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) == true)
                Model.EditPath = dialog.SelectedPath;
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var newLocation = new LocationModel();
            Model.Locations.Add(newLocation);
            Model.SelectedLocationItem = newLocation;
            Model.HasChanges = true;
            EditNameTextBox.Focus();
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (!Model.HasSelectedLocation) return;

            Model.Locations.Remove(Model.SelectedLocationItem);
            Model.SelectedLocationItem = null;
            Model.HasChanges = true;
        }

        //private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}
        
        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            try {
                await viewModel.SaveChangesAsync();
            }
            catch (Exception error) {
                ShowError($"Failed to save publish locations! {error.Message}");
                return;
            }

            Application.Current.Dispatcher.Invoke(() => {
                Model.HasChanges = false;
                return DialogResult = true;
            });
        }

        #endregion
    }
}
