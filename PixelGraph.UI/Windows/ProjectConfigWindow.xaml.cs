using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows;

public partial class ProjectConfigWindow
{
    private readonly ILogger<ProjectConfigWindow> logger;


    public ProjectConfigWindow(IServiceProvider provider)
    {
        logger = provider.GetRequiredService<ILogger<ProjectConfigWindow>>();

        InitializeComponent();

        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        themeHelper.ApplyCurrent(this);

        Model.Initialize(provider);
    }

    private async void OnEditEncodingClick(object sender, RoutedEventArgs e)
    {
        var formatFactory = TextureFormat.GetFactory(Model.Format);

        var window = new TextureFormatWindow {
            Owner = this,
            Model = {
                Encoding = (PackEncoding)Model.Project.Input.Clone(),
                DefaultEncoding = formatFactory.Create(),
                EnableSampler = false,
            },
        };

        try {
            if (window.ShowDialog() != true) return;
        }
        catch (Exception error) {
            logger.LogError(error, "An unhandled exception occurred showing TextureFormatWindow!");
            await this.ShowMessageAsync("Error!", $"An unhandled error occurred! {error.UnfoldMessageString()}");
            return;
        }

        Model.Project.Input = (PackInputEncoding)window.Model.Encoding;
    }

    private async void OnOkButtonClick(object sender, RoutedEventArgs e)
    {
        try {
            await Model.SaveAsync();
        }
        catch (Exception error) {
            logger.LogError(error, "Failed to save project data!");
            await this.ShowMessageAsync("Error!", $"Failed to save project data! {error.UnfoldMessageString()}");
            return;
        }

        await Dispatcher.BeginInvoke(() => DialogResult = true);
    }
}