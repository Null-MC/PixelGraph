using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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

        private string GetArchiveFilename()
        {
            var saveFileDialog = new VistaSaveFileDialog {
                Title = "Save published archive",
                Filter = "ZIP Archive|*.zip|All Files|*.*",
                FileName = $"{VM.SelectedItem?.Name}.zip",
                AddExtension = true,
            };

            return saveFileDialog.ShowDialog() == true
                ? saveFileDialog.FileName : null;
        }

        private static string GetDirectoryName()
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

            if (VM.Archive) builder.AddArchiveOutput();
            else builder.AddFileOutput();

            var logReceiver = new LogReceiver();
            logReceiver.LogMessage += OnLogMessage;

            builder.Services.AddSingleton<ILogReceiver>(logReceiver);
            builder.Services.AddSingleton(typeof(ILogger<>), typeof(RedirectLogger<>));
            builder.Services.AddSingleton<ILogger, RedirectLogger>();

            await using var scope = builder.Build();
            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var packReader = scope.GetRequiredService<IResourcePackReader>();
            var graphBuilder = scope.GetRequiredService<ITextureGraphBuilder>();
            var publisher = scope.GetRequiredService<IPublisher>();

            reader.SetRoot(VM.RootDirectory);
            writer.SetRoot(destination);
            graphBuilder.UseGlobalOutput = true;

            AppendLog(LogLevel.None, "Preparing output directory...");
            writer.Prepare();

            var context = new ResourcePackContext {
                Input = await packReader.ReadInputAsync("input.yml"),
                Profile = await packReader.ReadProfileAsync(VM.SelectedItem.LocalFile),
            };

            AppendLog(LogLevel.None, "Publishing content...");
            await publisher.PublishAsync(context, VM.Clean, token);
        }

        private void AppendLog(LogLevel level, string message)
        {
            var item = new LogMessageItem {
                Message = message,
                Color = logBrushMap.Get(level, Brushes.LightSlateGray),
            };

            Application.Current.Dispatcher.BeginInvoke(() => {
                VM.OutputLog.Add(item);

                var border = (Border)VisualTreeHelper.GetChild(LogListBox, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            });
        }

        private async void OnPublishButtonClick(object sender, RoutedEventArgs e)
        {
            var destination = VM.Archive
                ? GetArchiveFilename()
                : GetDirectoryName();

            if (destination == null) return;

            if (!VM.PublishBegin(out var token)) return;

            try {
                // TODO: Wire-up cancellation token to cancel button
                await Task.Run(() => PublishAsync(destination, token));

                AppendLog(LogLevel.None, "Publish completed successfully.");
            }
            catch (TaskCanceledException) {
                AppendLog(LogLevel.Warning, "Publish Cancelled!");
            }
            catch (Exception error) {
                AppendLog(LogLevel.Error, $"Publish Failed! {error.Message}");
            }
            finally {
                VM.PublishEnd();
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            VM.Cancel();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            VM.ShowOutput = false;
        }

        private void OnLogMessage(object sender, LogEventArgs e)
        {
            AppendLog(e.Level, e.Message);
        }

        private static readonly Dictionary<LogLevel, Brush> logBrushMap = new Dictionary<LogLevel, Brush> {
            [LogLevel.Debug] = Brushes.LimeGreen,
            [LogLevel.Information] = Brushes.LightSkyBlue,
            [LogLevel.Warning] = Brushes.Yellow,
            [LogLevel.Error] = Brushes.OrangeRed,
            [LogLevel.Critical] = Brushes.Red,
            [LogLevel.Trace] = Brushes.Purple,
        };
    }
}
