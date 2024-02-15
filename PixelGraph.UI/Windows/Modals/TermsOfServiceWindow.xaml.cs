using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal.Utilities;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace PixelGraph.UI.Windows.Modals;

public partial class TermsOfServiceWindow
{
    private readonly ILogger<TermsOfServiceWindow> logger;


    public TermsOfServiceWindow(IServiceProvider provider)
    {
        logger = provider.GetRequiredService<ILogger<TermsOfServiceWindow>>();

        InitializeComponent();

        Model.Initialize(provider);

        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        themeHelper.ApplyCurrent(this);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Document.Document = RtfHelper.LoadDocument("PixelGraph.UI.Resources.TOS.rtf");

        foreach (var hyperlink in RtfHelper.GetVisuals(Document.Document).OfType<Hyperlink>())
            hyperlink.RequestNavigate += OnDocumentHyperlinkRequestNavigate;
    }

    private async void OnAcceptButtonClick(object sender, RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(Model.Data);

        await Model.Data.SetResultAsync(true);
        await Dispatcher.BeginInvoke(() => DialogResult = true);
    }

    private async void OnDeclineButtonClick(object sender, RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(Model.Data);

        await Model.Data.SetResultAsync(false);
        await Dispatcher.BeginInvoke(() => DialogResult = false);
    }

    private async void OnDocumentHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try {
            ProcessHelper.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
        catch (Exception error) {
            logger.LogError(error, "Failed to open document link!");
            await this.ShowMessageAsync("Error!", $"Failed to open document link! {error.UnfoldMessageString()}");
        }
    }
}