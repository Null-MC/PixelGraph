using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.UI.Internal.Logging;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows;

public partial class PublishOutputWindow : IDisposable
{
    public PublishOutputWindow(IServiceProvider provider)
    {
        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        var appSettings = provider.GetRequiredService<IAppSettingsManager>();

        InitializeComponent();

        themeHelper.ApplyCurrent(this);

        Model.Initialize(provider);
        ArgumentNullException.ThrowIfNull(Model.Data);

        Model.Data.IsLoading = true;

        Model.Data.StateChanged += OnStatusChanged;
        Model.Data.LogAppended += OnLogAppended;

        Model.Data.CloseOnComplete = appSettings.Data.PublishCloseOnComplete;

        Model.Data.IsLoading = false;
    }

    public void Dispose()
    {
        Model?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnStatusChanged(object? sender, PublishStatus e)
    {
        ArgumentNullException.ThrowIfNull(Model.Data);

        var data = Model.Data;
        Dispatcher.BeginInvoke(() => {
            data.IsAnalyzing = e.IsAnalyzing;
            data.Progress = e.Progress;
        }, DispatcherPriority.DataBind);
    }

    private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(Model.Data);

        Model.Data.IsActive = true;

        try {
            var success = await Model.Data.PublishAsync();
            if (success && Model.Data.CloseOnComplete) DialogResult = true;
        }
        catch (OperationCanceledException) {
            if (IsLoaded) DialogResult = false;
        }
        finally {
            await Dispatcher.BeginInvoke(() => Model.Data.IsActive = false);
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Model.Cancel();
        Model.Dispose();
    }

    private void OnLogAppended(object? sender, LogEventArgs e)
    {
        LogList.Append(e.Level, e.Message);
    }

    private void OnCancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Model.Cancel();
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Model.Cancel();
        DialogResult = true;
    }
}
