using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class PublishWindow : IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly ILogger logger;


        public PublishWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();

            logger = provider.GetRequiredService<ILogger<PublishWindow>>();
            var settings = provider.GetRequiredService<IAppSettings>();

            VM.CloseOnComplete = settings.Data.PublishCloseOnComplete;
            VM.LogList.Appended += OnLogListAppended;
        }

        private async Task PublishAsync(string destination, CancellationToken token)
        {
            var builder = provider.GetRequiredService<IServiceBuilder>();
            builder.AddFileInput();

            if (VM.Archive) builder.AddArchiveOutput();
            else builder.AddFileOutput();

            var logReceiver = builder.AddLoggingRedirect();
            logReceiver.LogMessage += OnLogMessage;

            await using var scope = builder.Build();
            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var packReader = scope.GetRequiredService<IResourcePackReader>();
            var publisher = scope.GetRequiredService<IPublisher>();

            reader.SetRoot(VM.RootDirectory);
            writer.SetRoot(destination);

            VM.LogList.BeginAppend(LogLevel.None, "Preparing output directory...");
            writer.Prepare();

            var context = new ResourcePackContext {
                Input = await packReader.ReadInputAsync("input.yml"),
                Profile = await packReader.ReadProfileAsync(VM.Profile.LocalFile),
                UseGlobalOutput = true,
            };

            VM.LogList.BeginAppend(LogLevel.None, "Publishing content...");
            await publisher.PublishAsync(context, VM.Clean, token);
        }

        public void Dispose()
        {
            VM?.Dispose();
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            logger.LogInformation("Publishing profile '{Name}'...", VM.Profile.Name);
            VM.IsActive = true;

            try {
                await Task.Run(() => PublishAsync(VM.Destination, VM.Token), VM.Token);

                logger.LogInformation("Publish successful.");
                VM.LogList.BeginAppend(LogLevel.None, "Publish completed successfully.");

                if (VM.CloseOnComplete) DialogResult = true;
            }
            catch (TaskCanceledException) {
                logger.LogWarning("Publish cancelled.");
                VM.LogList.BeginAppend(LogLevel.Warning, "Publish Cancelled!");
                DialogResult = false;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to publish resource pack!");
                VM.LogList.BeginAppend(LogLevel.Error, $"Publish Failed! {error.UnfoldMessageString()}");
            }
            finally {
                await Application.Current.Dispatcher.BeginInvoke(() => VM.IsActive = false);
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            VM.Cancel();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnLogMessage(object sender, LogEventArgs e)
        {
            VM.LogList.BeginAppend(e.Level, e.Message);
        }

        private void OnLogListAppended(object sender, EventArgs e)
        {
            LogList.ScrollToEnd();
        }

        private async void OnCloseCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var settings = provider.GetRequiredService<IAppSettings>();
            settings.Data.PublishCloseOnComplete = VM.CloseOnComplete;

            await settings.SaveAsync();
        }
    }
}
