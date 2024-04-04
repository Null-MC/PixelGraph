using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Windows;

public partial class ResourceLocationsWindow
{
    private ResourceLocationsViewModel Model {get;}


    public ResourceLocationsWindow(IServiceProvider provider)
    {
        Model = provider.GetRequiredService<ResourceLocationsViewModel>();
        DataContext = Model;

        InitializeComponent();

        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        themeHelper.ApplyCurrent(this);
    }

    private async Task SaveAsync()
    {
        try {
            await Model.SaveAsync();
        }
        catch (Exception error) {
            await this.ShowMessageAsync("Error!", $"Failed to save resource locations! {error.Message}");
            return;
        }

        await Dispatcher.BeginInvoke(() => {
            Model.HasChanges = false;
            DialogResult = true;
        });
    }

    private bool TryGetFilenames(bool allowMultiple, out string[] files)
    {
        var dialog = new VistaOpenFileDialog {
            Filter = "Archive File|*.jar;*.zip|Jar Files|*.jar|Zip Files|*.zip|All Files|*.*",
            Multiselect = allowMultiple,
        };

        var result = dialog.ShowDialog(this) == true;
        files = dialog.FileNames;
        return result;
    }

    //private void ShowError(string message)
    //{
    //    Application.Current.Dispatcher.Invoke(() => {
    //        MessageBox.Show(this, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
    //    });
    //}

    #region Events

    private async void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (DialogResult.HasValue || !Model.HasChanges) return;

        var result = MessageBox.Show(this, "Would you like to save your changes?", "Warning!", MessageBoxButton.YesNoCancel);

        switch (result) {
            case MessageBoxResult.Yes:
                await SaveAsync();
                break;
            case MessageBoxResult.Cancel:
                e.Cancel = true;
                break;
        }
    }

    private void OnLocationListBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
        var r = VisualTreeHelper.HitTest(LocationsListBox, e.GetPosition(LocationsListBox));
        if (r.VisualHit.GetType() != typeof(ListBoxItem)) LocationsListBox.UnselectAll();
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        if (TryGetFilenames(true, out var files))
            Model.AddFiles(files);
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        Model.RemoveSelected();
    }

    private void OnPathBrowseClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetFilenames(true, out var files)) return;

        Model.EditFile = files.FirstOrDefault();
    }
        
    private async void OnOkButtonClick(object sender, RoutedEventArgs e)
    {
        await SaveAsync();
    }

    #endregion
}
