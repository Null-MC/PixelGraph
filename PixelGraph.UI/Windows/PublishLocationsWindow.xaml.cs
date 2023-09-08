using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows;

public partial class PublishLocationsWindow
{
    private readonly PublishLocationsViewModel viewModel;


    public PublishLocationsWindow(IServiceProvider provider)
    {
        DataContext = viewModel = new PublishLocationsViewModel(provider);

        InitializeComponent();

        var themeHelper = provider.GetRequiredService<IThemeHelper>();
        themeHelper.ApplyCurrent(this);
    }

    private async Task SaveAsync()
    {
        try {
            await viewModel.SaveAsync();
        }
        catch (Exception error) {
            ShowError($"Failed to save publish locations! {error.Message}");
            return;
        }

        await Dispatcher.BeginInvoke(() => {
            viewModel.HasChanges = false;
            DialogResult = true;
        });
    }

    private void ShowError(string message)
    {
        Application.Current.Dispatcher.Invoke(() => {
            MessageBox.Show(this, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }

    #region Events

    private async void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (DialogResult.HasValue || !viewModel.HasChanges) return;

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

    private void OnPathBrowseClick(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog {
            Description = "Please select a folder.",
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog(this) == true)
            viewModel.EditPath = dialog.SelectedPath;
    }

    private void OnLocationListBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
        var r = VisualTreeHelper.HitTest(LocationsListBox, e.GetPosition(LocationsListBox));
        if (r.VisualHit.GetType() != typeof(ListBoxItem)) LocationsListBox.UnselectAll();
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        viewModel.AddNew();
        EditNameTextBox.Focus();
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        viewModel.RemoveSelected();
    }

    private void OnDuplicateClick(object sender, RoutedEventArgs e)
    {
        viewModel.DuplicateSelected();
    }
        
    private async void OnOkButtonClick(object sender, RoutedEventArgs e)
    {
        await SaveAsync();
    }

    #endregion
}