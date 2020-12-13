using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
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


        public PackProfilesWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        private async Task CreateNewProfile()
        {
            var dialog = new SaveFileDialog {
                Title = "Create a new pack properties file",
                Filter = "Pack YAML|*.pack.yml|All Files|*.*",
                AddExtension = true,
                FileName = "default",
            };

            var result = dialog.ShowDialog(this);
            if (result != true) return;

            if (!dialog.FileName.StartsWith(VM.RootDirectory)) {
                // ERROR: show error message
                return;
            }

            var localFile = dialog.FileName
                .Substring(VM.RootDirectory.Length)
                .TrimStart('\\', '/');

            var packProfile = new ResourcePackProfileProperties {
                //InputFormat = EncodingProperties.Raw,
                Output = {
                    Format = TextureEncoding.Format_Lab13,
                },
            };

            var writer = provider.GetRequiredService<IOutputWriter>();
            var packWriter = provider.GetRequiredService<IResourcePackWriter>();

            writer.SetRoot(VM.RootDirectory);
            await packWriter.WriteAsync(localFile, packProfile);

            var profile = CreateProfileItem(localFile);
            Application.Current.Dispatcher.Invoke(() => {
                VM.Profiles.Add(profile);
                VM.SelectedProfileItem = profile;
            });
        }

        private void DeleteSelectedProfile()
        {
            var profile = VM.SelectedProfileItem;
            if (profile?.LocalFile == null) return;

            var writer = provider.GetRequiredService<IOutputWriter>();
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
                ShowError($"Failed to save pack profile '{packProfile.LocalFile}'! {error.Message}");
            }
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
            throw new NotImplementedException();
        }

        private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
        {
            DeleteSelectedProfile();
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

        private void OnTextureResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is Button button)) return;
            var cell = button.FindParent<DataGridCell>();
            var mapping = (OutputChannelMapping) cell?.DataContext;
            mapping?.Clear();
        }

        #endregion
    }
}
