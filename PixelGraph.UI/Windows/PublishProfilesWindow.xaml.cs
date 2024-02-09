using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Windows;

public partial class PackProfilesWindow
{
    private readonly ILogger<PackProfilesWindow> logger;


    public PackProfilesWindow(IServiceProvider provider)
    {
        logger = provider.GetRequiredService<ILogger<PackProfilesWindow>>();

        InitializeComponent();

        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        themeHelper.ApplyCurrent(this);

        Model.Initialize(provider);
    }

    private bool TryGenerateGuid(Guid? currentValue, out Guid id)
    {
        if (!currentValue.HasValue || MessageBox.Show(this, "A value already exists! Are you sure you want to replace it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
            id = Guid.NewGuid();
            return true;
        }

        id = Guid.Empty;
        return false;
    }

    #region Events

    private void OnNewProfileClick(object sender, RoutedEventArgs e)
    {
        Model.CreateNewProfile();
    }

    private void OnDuplicateProfileClick(object sender, RoutedEventArgs e)
    {
        Model.CloneSelectedProfile();
    }

    private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
    {
        //var result = MessageBox.Show(this, "Are you sure? This operation cannot be undone.", "Warning", MessageBoxButton.OKCancel);
        //if (result != MessageBoxResult.OK) return;

        Model.RemoveSelected();
    }

    private void OnProfileListBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
        var r = VisualTreeHelper.HitTest(ProfileListBox, e.GetPosition(ProfileListBox));
        if (r.VisualHit.GetType() != typeof(ListBoxItem)) ProfileListBox.UnselectAll();
    }

    private void OnGenerateHeaderUuid(object sender, RoutedEventArgs e)
    {
        var profile = Model.SelectedProfile;
        if (profile == null) return;

        if (TryGenerateGuid(profile.PackHeaderUuid, out var newGuid))
            profile.PackHeaderUuid = newGuid;
    }

    private void OnGenerateModuleUuid(object sender, RoutedEventArgs e)
    {
        var profile = Model.SelectedProfile;
        if (profile == null) return;

        if (TryGenerateGuid(profile.PackModuleUuid, out var newGuid))
            profile.PackModuleUuid = newGuid;
    }

    private void OnEncodingSamplerKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        Model.EditEncodingSampler = null;
    }

    private void OnImageEncodingKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        Model.EditImageEncoding = null;
    }

    private async void OnEditEncodingClick(object sender, RoutedEventArgs e)
    {
        var profile = Model.SelectedProfile?.Profile;
        if (Model.SelectedProfile == null || profile == null) return;

        var formatFactory = TextureFormat.GetFactory(Model.SelectedProfile.TextureFormat);

        try {
            var window = new TextureFormatWindow {
                Owner = this,
                Model = {
                    Encoding = (PackEncoding)profile.Encoding.Clone(),
                    DefaultEncoding = formatFactory.Create(),
                    DefaultSampler = Model.EditEncodingSampler,
                    EnableSampler = true,
                },
            };

            if (window.ShowDialog() != true) return;

            profile.Encoding = (PackOutputEncoding)window.Model.Encoding;
        }
        catch (Exception error) {
            logger.LogError(error, "An unhandled exception occurred in TextureFormatWindow!");
            await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
        }
    }

    private async void OnSaveButtonClick(object sender, RoutedEventArgs e)
    {
        try {
            await Model.SaveAsync();
        }
        catch (Exception error) {
            logger.LogError(error, "Failed to save project file!");
            await this.ShowMessageAsync("Error!", "Failed to save project file!");
            return;
        }

        await Dispatcher.BeginInvoke(() => DialogResult = true);
    }

    #endregion
}