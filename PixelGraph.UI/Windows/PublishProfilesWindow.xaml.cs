using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.ViewData;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Windows
{
    public partial class PackProfilesWindow
    {
        private readonly IServiceProvider provider;
        private readonly ILogger logger;


        public PackProfilesWindow(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<PackProfilesWindow>>();

            InitializeComponent();
        }

        private async Task CreateNewProfile()
        {
            var filename = GetSaveName(VM.RootDirectory, "default");
            if (filename == null) return;

            var profile = new ResourcePackProfileProperties {
                LocalFile = filename[VM.RootDirectory.Length..].TrimStart('\\', '/'),
                Encoding = {
                    Format = TextureEncoding.Format_Lab13,
                },
            };

            await SaveAsync(profile);

            var profileItem = CreateProfileItem(profile.LocalFile);
            Application.Current.Dispatcher.Invoke(() => {
                VM.Profiles.Add(profileItem);
                VM.SelectedProfileItem = profileItem;
            });
        }

        private void CloneSelectedProfile()
        {
            var profileItem = VM.SelectedProfileItem;
            if (profileItem?.LocalFile == null) return;

            var path = Path.GetDirectoryName(profileItem.LocalFile);
            var filename = GetSaveName(path, profileItem.Name);
            if (filename == null) return;

            var srcFullFile = Path.Join(VM.RootDirectory, profileItem.LocalFile);
            File.Copy(srcFullFile, filename);

            var cloneLocalFile = filename[VM.RootDirectory.Length..].TrimStart('\\', '/');

            var cloneProfileItem = CreateProfileItem(cloneLocalFile);
            Application.Current.Dispatcher.Invoke(() => {
                VM.Profiles.Add(cloneProfileItem);
                VM.SelectedProfileItem = cloneProfileItem;
            });
        }

        private void DeleteSelectedProfile()
        {
            var profile = VM.SelectedProfileItem;
            if (profile?.LocalFile == null) return;

            var writer = provider.GetRequiredService<IOutputWriter>();
            writer.SetRoot(VM.RootDirectory);
            writer.Delete(profile.LocalFile);

            VM.Profiles.Remove(profile);
        }

        private static ProfileItem CreateProfileItem(string filename)
        {
            var fileName = Path.GetFileName(filename);

            var item = new ProfileItem {
                Name = fileName[..^9],
                LocalFile = filename,
            };

            return item;
        }

        private async Task SaveAsync(ResourcePackProfileProperties packProfile)
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileOutput();

            try {
                await using var scope = scopeBuilder.Build();
                var writer = scope.GetRequiredService<IOutputWriter>();
                var packWriter = scope.GetRequiredService<IResourcePackWriter>();

                writer.SetRoot(VM.RootDirectory);
                await packWriter.WriteAsync(packProfile.LocalFile, packProfile);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to save pack profile!");
                ShowError($"Failed to save pack profile '{packProfile.LocalFile}'! {error.Message}");
            }
        }

        private string GetSaveName(string path, string name)
        {
            var dialog = new SaveFileDialog {
                Title = "Create a new pack properties file",
                Filter = "Pack YAML|*.pack.yml|All Files|*.*",
                InitialDirectory = path,
                AddExtension = true,
                FileName = name,
            };

            var result = dialog.ShowDialog(this);
            if (result != true) return null;

            if (!dialog.FileName.StartsWith(VM.RootDirectory)) {
                ShowError("Pack profile should be saved in project root directory!");
                return null;
            }

            return dialog.FileName;
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
            });
        }

        #region Events

        private async void OnNewProfileClick(object sender, RoutedEventArgs e)
        {
            await CreateNewProfile();
        }

        private void OnDuplicateProfileClick(object sender, RoutedEventArgs e)
        {
            CloneSelectedProfile();
        }

        private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this, "Are you sure? This operation cannot be undone.", "Warning", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            try {
                DeleteSelectedProfile();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to delete profile!");
                MessageBox.Show("Failed to delete profile!");
            }
        }

        private async void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Wait for existing save!

            if (VM.SelectedProfileItem != null) {
                try {
                    var packReader = provider.GetRequiredService<IResourcePackReader>();
                    VM.LoadedProfile = await packReader.ReadProfileAsync(VM.SelectedProfileItem.LocalFile);
                    //vm.LoadedFilename = vm.SelectedProfileItem.Filename;
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to delete pack profile!");
                    ShowError($"Failed to load pack profile '{VM.SelectedProfileItem.LocalFile}'! {error.Message}");
                }
            }
            else {
                VM.LoadedProfile = null;
            }
        }

        private void OnProfileListBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(ProfileListBox, e.GetPosition(ProfileListBox));
            if (r.VisualHit.GetType() != typeof(ListBoxItem)) ProfileListBox.UnselectAll();
        }

        private async void OnDataChanged(object sender, EventArgs e)
        {
            await SaveAsync(VM.LoadedProfile);
        }

        private void OnGenerateHeaderUuid(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "A value already exists! Are you sure you want to replace it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                VM.PackHeaderUuid = Guid.NewGuid();
        }

        private void OnGenerateModuleUuid(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "A value already exists! Are you sure you want to replace it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                VM.PackModuleUuid = Guid.NewGuid();
        }

        private void OnEncodingSamplerKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            VM.EditEncodingSampler = null;
        }

        private void OnImageEncodingKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            VM.EditImageEncoding = null;
        }

        private void OnEncodingDataGridMouseWheelPreview(object sender, MouseWheelEventArgs e)
        {
            EncodingScrollViewer.ScrollToVerticalOffset(EncodingScrollViewer.ContentVerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnEncodingDataGridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            if (TextureEncodingDataGrid.SelectedValue is not TextureChannelMapping channel) return;

            channel.Clear();
        }

        #endregion
    }
}
