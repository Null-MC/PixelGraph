using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.Publishing;
using PixelGraph.Common.Textures;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class PublishWindow
    {
        private readonly IServiceProvider provider;


        public PublishWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        private async void OnPublishButtonClick(object sender, RoutedEventArgs e)
        {
            var destination = VM.Archive
                ? GetArchiveFilename()
                : GetDirectoryName();

            if (destination == null || !VM.PublishBegin()) return;

            try {
                // TODO: Wire-up cancellation token to cancel button
                await Task.Run(() => PublishAsync(destination, CancellationToken.None));

                Application.Current.Dispatcher.Invoke(() => {
                    VM.OutputLog.Add("Publish completed successfully.");
                });
            }
            catch (Exception error) {
                Application.Current.Dispatcher.Invoke(() => {
                    VM.OutputLog.Add($"Publish Failed! {error.Message}");
                });
            }
            finally {
                VM.PublishEnd();
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (VM.IsBusy) VM.Cancel();
            else Close();
        }

        private string GetArchiveFilename()
        {
            var saveFileDialog = new VistaSaveFileDialog {
                Title = "Save published archive",
                Filter = "ZIP Archive|*.zip|All Files|*.*",
                FileName = VM.SelectedItem?.Name,
                AddExtension = true,
            };

            return saveFileDialog.ShowDialog() == true
                ? saveFileDialog.FileName : null;
        }

        private string GetDirectoryName()
        {
            var folderDialog = new VistaFolderBrowserDialog {
                Description = "Destination for published resource pack content.",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
            };

            return folderDialog.ShowDialog() == true
                ? folderDialog.SelectedPath : null;
        }

        private async Task PublishAsync(string destination, CancellationToken token)
        {
            if (VM.SelectedItem == null) return;

            var builder = provider.GetRequiredService<IServiceBuilder>();
            builder.AddFileInput();
            //builder.AddLoggingRedirect();

            if (VM.Archive) builder.AddArchiveOutput();
            else builder.AddFileOutput();

            await using var scope = builder.Build();
            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var packReader = scope.GetRequiredService<IResourcePackReader>();
            var graphBuilder = scope.GetRequiredService<ITextureGraphBuilder>();
            var publisher = scope.GetRequiredService<IPublisher>();

            reader.SetRoot(VM.RootDirectory);
            writer.SetRoot(destination);
            graphBuilder.UseGlobalOutput = true;

            Application.Current.Dispatcher.Invoke(() => {
                VM.OutputLog.Add("Preparing output directory...");
            });

            writer.Prepare();

            var context = new ResourcePackContext {
                Input = await packReader.ReadInputAsync("input.yml"),
                Profile = await packReader.ReadProfileAsync(VM.SelectedItem.LocalFile),
            };

            Application.Current.Dispatcher.Invoke(() => {
                VM.OutputLog.Add("Publishing content...");
            });

            await publisher.PublishAsync(context, VM.Clean, token);
        }
    }
}
