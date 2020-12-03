using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
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
        private readonly IAppSettings settings;


        public PublishWindow(IServiceProvider provider)
        {
            this.provider = provider;

            settings = provider.GetRequiredService<IAppSettings>();

            InitializeComponent();

            VM.CloseOnComplete = settings.PublishCloseOnComplete;
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
            var graphBuilder = scope.GetRequiredService<ITextureGraphBuilder>();
            var publisher = scope.GetRequiredService<IPublisher>();

            reader.SetRoot(VM.RootDirectory);
            writer.SetRoot(destination);
            graphBuilder.UseGlobalOutput = true;

            VM.LogList.BeginAppend(LogLevel.None, "Preparing output directory...");
            writer.Prepare();

            var context = new ResourcePackContext {
                Input = await packReader.ReadInputAsync("input.yml"),
                Profile = await packReader.ReadProfileAsync(VM.Profile.LocalFile),
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
            VM.IsActive = true;

            try {
                await Task.Run(() => PublishAsync(VM.Destination, VM.Token), VM.Token);

                VM.LogList.BeginAppend(LogLevel.None, "Publish completed successfully.");
                if (VM.CloseOnComplete) DialogResult = true;
            }
            catch (TaskCanceledException) {
                VM.LogList.BeginAppend(LogLevel.Warning, "Publish Cancelled!");
                DialogResult = false;
            }
            catch (Exception error) {
                VM.LogList.BeginAppend(LogLevel.Error, $"Publish Failed! {error.Message}");
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
            settings.PublishCloseOnComplete = VM.CloseOnComplete;
            await settings.SaveAsync();
        }
    }
}
