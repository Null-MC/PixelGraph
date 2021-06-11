using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class PublishOutputWindow : IDisposable
    {
        //private readonly IServiceProvider provider;
        private readonly ILogger<PublishOutputWindow> logger;
        private readonly PublishOutputViewModel viewModel;


        public PublishOutputWindow(IServiceProvider provider)
        {
            //this.provider = provider;

            logger = provider.GetRequiredService<ILogger<PublishOutputWindow>>();
            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            var settings = provider.GetRequiredService<IAppSettings>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new PublishOutputViewModel(provider) {
                Model = Model,
            };

            viewModel.LogAppended += OnLogAppended;

            Model.CloseOnComplete = settings.Data.PublishCloseOnComplete;
            //Model.LogEvent += OnLogMessage;
        }

        public void Dispose()
        {
            viewModel?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            logger.LogInformation("Publishing profile '{Name}'...", Model.Profile.Name);
            Model.IsActive = true;

            try {
                await Task.Run(() => viewModel.PublishAsync());

                logger.LogInformation("Publish successful.");
                LogList.Append(LogLevel.None, "Publish completed successfully.");

                if (Model.CloseOnComplete) DialogResult = true;
            }
            catch (OperationCanceledException) {
                logger.LogWarning("Publish cancelled.");
                LogList.Append(LogLevel.Warning, "Publish Cancelled!");
                DialogResult = false;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to publish resource pack!");
                LogList.Append(LogLevel.Error, $"Publish Failed! {error.UnfoldMessageString()}");
            }
            finally {
                await Application.Current.Dispatcher.BeginInvoke(() => Model.IsActive = false);
            }
        }

        //public void Append(LogLevel level, string text)
        //{
        //    this.BeginInvoke(() => {
        //        viewModel.Append(level, text);
        //    });
        //}

        private void OnLogAppended(object sender, LogEventArgs e)
        {
            LogList.Append(e.Level, e.Message);
        }

        private async void OnCloseCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            await viewModel.UpdateSettingsAsync();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
