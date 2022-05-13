using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class PublishOutputWindow : IDisposable
    {
        public PublishOutputWindow(IServiceProvider provider)
        {
            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            var appSettings = provider.GetRequiredService<IAppSettingsManager>();

            InitializeComponent();

            themeHelper.ApplyCurrent(this);
            
            Model.StateChanged += OnStatusChanged;
            Model.LogAppended += OnLogAppended;

            Model.IsLoading = true;
            Model.CloseOnComplete = appSettings.Data.PublishCloseOnComplete;

            Model.Initialize(provider);
            Model.IsLoading = false;
        }

        private void OnStatusChanged(object sender, PublishStatus e)
        {
            Dispatcher.BeginInvoke(() => {
                Model.IsAnalyzing = e.IsAnalyzing;
                Model.Progress = e.Progress;
            }, DispatcherPriority.DataBind);
        }

        public void Dispose()
        {
            Model?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Model.IsActive = true;

            try {
                var success = await Model.PublishAsync();
                if (success && Model.CloseOnComplete) DialogResult = true;
            }
            catch (OperationCanceledException) {
                if (IsLoaded) DialogResult = false;
            }
            finally {
                await Dispatcher.BeginInvoke(() => Model.IsActive = false);
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            Model.Cancel();
            Model.Dispose();
        }

        private void OnLogAppended(object sender, LogEventArgs e)
        {
            LogList.Append(e.Level, e.Message);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Model.Cancel();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Model.Cancel();
            DialogResult = true;
        }
    }
}
