using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class PublishOutputWindow : IDisposable
    {
        private readonly IAppSettings appSettings;
        private readonly ILogger<PublishOutputWindow> logger;
        private readonly PublishOutputViewModel viewModel;


        public PublishOutputWindow(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<PublishOutputWindow>>();
            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            appSettings = provider.GetRequiredService<IAppSettings>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new PublishOutputViewModel(provider) {
                Model = Model,
            };

            viewModel.LogAppended += OnLogAppended;

            Model.IsLoading = true;
            Model.CloseOnComplete = appSettings.Data.PublishCloseOnComplete;
            Model.IsLoading = false;
        }

        public void Dispose()
        {
            viewModel?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var timer = Stopwatch.StartNew();
            logger.LogInformation("Publishing profile '{Name}'...", Model.Profile.Name);

            var concurrency = appSettings.Data.Concurrency ?? Environment.ProcessorCount;
            LogList.Append(LogLevel.Debug, $"  Concurrency: {concurrency:N0}");
            Model.IsActive = true;

            try {
                await Task.Run(() => viewModel.PublishAsync());
                timer.Stop();

                logger.LogInformation("Publish successful. Duration: {Elapsed}", timer.Elapsed);
                LogList.Append(LogLevel.None, "Publish completed successfully.");

                if (Model.CloseOnComplete) DialogResult = true;
            }
            catch (OperationCanceledException) {
                timer.Stop();
                logger.LogWarning("Publish cancelled.");
                LogList.Append(LogLevel.Warning, "Publish Cancelled!");
                if (IsLoaded) DialogResult = false;
            }
            catch (Exception error) {
                timer.Stop();
                logger.LogError(error, "Failed to publish resource pack!");
                LogList.Append(LogLevel.Error, $"Publish Failed! {error.UnfoldMessageString()}");
            }
            finally {
                LogList.Append(LogLevel.Debug, $"Duration: {timer.Elapsed:g}");
                await Application.Current.Dispatcher.BeginInvoke(() => Model.IsActive = false);
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            viewModel.Cancel();
            viewModel.Dispose();
        }

        private void OnLogAppended(object sender, LogEventArgs e)
        {
            LogList.Append(e.Level, e.Message);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
            DialogResult = true;
        }
    }
}
