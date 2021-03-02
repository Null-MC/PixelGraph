using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class PublishLocationsWindow
    {
        private readonly IServiceProvider provider;


        public PublishLocationsWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
            });
        }

        #region Events

        private async void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (DialogResult.HasValue || !VM.HasChanges) return;

            var result = MessageBox.Show(this, "Would you like to save your changes?", "Warning!", MessageBoxButton.YesNoCancel);

            switch (result) {
                case MessageBoxResult.Yes:
                    await SaveChangesAsync();
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
                VM.EditPath = dialog.SelectedPath;
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var newLocation = new LocationViewModel();
            VM.Locations.Add(newLocation);
            VM.SelectedLocationItem = newLocation;
            VM.HasChanges = true;
            EditNameTextBox.Focus();
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (!VM.HasSelectedLocation) return;

            VM.Locations.Remove(VM.SelectedLocationItem);
            VM.SelectedLocationItem = null;
            VM.HasChanges = true;
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async Task SaveChangesAsync(CancellationToken token = default)
        {
            var locationMgr = provider.GetRequiredService<IPublishLocationManager>();
            await locationMgr.SaveAsync(VM.Locations.Select(x => x.DataSource), token);
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            try {
                await SaveChangesAsync();
            }
            catch (Exception error) {
                ShowError($"Failed to save publish locations! {error.Message}");
                return;
            }

            Application.Current.Dispatcher.Invoke(() => {
                VM.HasChanges = false;
                return DialogResult = true;
            });
        }

        #endregion
    }
}
