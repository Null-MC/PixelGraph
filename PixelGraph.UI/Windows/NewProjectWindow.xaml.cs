using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewProjectWindow
    {
        private readonly IServiceProvider provider;
        private readonly ILogger logger;


        public NewProjectWindow(IServiceProvider provider)
        {
            this.provider = provider;
            logger = provider.GetRequiredService<ILogger<NewProjectWindow>>();

            InitializeComponent();
        }

        private bool CreateDirectory()
        {
            if (Directory.Exists(VM.Location)) return true;

            try {
                Directory.CreateDirectory(VM.Location);
                return true;
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to create new project directory \"{VM.Location}\"!");
                MessageBox.Show(this, "Failed to create new project directory!", "Error!", MessageBoxButton.OK);
                VM.SetState(NewProjectStates.Location);
                return false;
            }
        }

        private async Task CreatePackFilesAsync()
        {
            var writer = provider.GetRequiredService<IOutputWriter>();
            var packWriter = provider.GetRequiredService<IResourcePackWriter>();
            writer.SetRoot(VM.Location);

            var packInput = new ResourcePackInputProperties {
                Format = VM.ContentFormat,
            };

            await packWriter.WriteAsync("input.yml", packInput);

            if (VM.CreateDefaultProfile) {
                var packProfile = new ResourcePackProfileProperties {
                    Description = "My new resource pack",
                    Encoding = {
                        Format = TextureEncoding.Format_Lab13,
                    },
                    Edition = "Java",
                    Format = 6,
                };

                await packWriter.WriteAsync("default.pack.yml", packProfile);
            }
        }

        #region Events

        private void OnLocationBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) != true) return;
            VM.Location = dialog.SelectedPath;
        }

        private void OnLocationCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnLocationNextClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(VM.Location)) {
                MessageBox.Show(this, "Please select a location for your project content!", "Error!", MessageBoxButton.OK);
                return;
            }

            VM.SetState(NewProjectStates.Format);
        }

        private void OnFormatBackClick(object sender, RoutedEventArgs e)
        {
            VM.SetState(NewProjectStates.Location);
        }

        private void OnFormatNextClick(object sender, RoutedEventArgs e)
        {
            VM.SetState(NewProjectStates.Review);
        }

        private void OnReviewBackClick(object sender, RoutedEventArgs e)
        {
            VM.SetState(NewProjectStates.Format);
        }

        private async void OnReviewCreateClick(object sender, RoutedEventArgs e)
        {
            if (VM.EnablePackImport && !VM.ImportFromDirectory && !VM.ImportFromArchive) {
                MessageBox.Show(this, "Please select the type of source you would like to import project content from!", "Error!", MessageBoxButton.OK);
                return;
            }

            if (!CreateDirectory()) return;

            await CreatePackFilesAsync();

            DialogResult = true;
        }

        #endregion
    }
}
