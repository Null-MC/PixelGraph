using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.UI.Internal.Logging;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows;

public partial class PublishOutputWindow : IDisposable
{
    public PublishOutputViewModel Model {get;}


    public PublishOutputWindow(IServiceProvider provider)
    {
        Model = provider.GetRequiredService<PublishOutputViewModel>();
        DataContext = Model;

        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        var appSettings = provider.GetRequiredService<IAppSettingsManager>();

        InitializeComponent();

        themeHelper.ApplyCurrent(this);

        Model.IsLoading = true;

        Model.StateChanged += OnStatusChanged;
        Model.LogAppended += OnLogAppended;

        Model.CloseOnComplete = appSettings.Data.PublishCloseOnComplete;

        Model.Clean = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        Model.IsLoading = false;

        Model.IsInitializing = false;
    }

    public void Dispose()
    {
        Model.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnStatusChanged(object? sender, PublishStatus e)
    {
        //ArgumentNullException.ThrowIfNull(Model);

        Dispatcher.BeginInvoke(() => {
            Model.IsAnalyzing = e.IsAnalyzing;
            Model.Progress = e.Progress;
        }, DispatcherPriority.DataBind);
    }

    private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        //ArgumentNullException.ThrowIfNull(Model);

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
