using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.UI.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Controls;

public partial class RecentProjectsControl
{
    private ILogger<RecentProjectsControl>? logger;

    public event EventHandler<TileClickedEventArgs>? TileClicked;


    public RecentProjectsControl()
    {
        InitializeComponent();
    }

    public void Initialize(IServiceProvider provider)
    {
        logger = provider.GetRequiredService<ILogger<RecentProjectsControl>>();

        Model.Initialize(provider);

        Model.Data.MissingImage = CreateMissingImage();
    }

    public async Task AppendAsync(string filename, CancellationToken token = default)
    {
        await Model.Data.AppendProjectAsync(filename, token);
        await Model.Data.UpdateModelListAsync(Dispatcher, token);
    }

    private async Task OpenProjectAsync(string filename)
    {
        if (!File.Exists(filename)) {
            var window = Window.GetWindow(this);
            if (window != null) MessageBox.Show(window, "The selected project file could not be found!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            await Model.Data.RemoveAsync(filename);
            await Model.Data.UpdateModelListAsync(Dispatcher);
            return;
        }

        TileClicked?.Invoke(this, new TileClickedEventArgs {Filename = filename});
    }

    private async void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        try {
            await Task.Run(Model.Data.LoadAsync);
        }
        catch (Exception error) {
            logger?.LogError(error, "Failed to load recent projects!");
            var window = Window.GetWindow(this);
            if (window != null) await Dispatcher.BeginInvoke(() => MessageBox.Show(window, "Failed to load recent project list!", "Error"));
        }

        await Task.Run(() => Model.Data.UpdateModelListAsync(Dispatcher));
    }

    private void OnTileClick(object sender, RoutedEventArgs e)
    {
        if ((e.Source as Tile)?.DataContext is not ProjectTileModel project) return;

        if (project.Filename != null)
            OnTileClicked(project.Filename);
    }

    private async void OnTileClicked(string filename)
    {
        await OpenProjectAsync(filename);
    }

    private static ImageSource CreateMissingImage()
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CreateOptions = BitmapCreateOptions.DelayCreation;
        image.DecodePixelWidth = 64;
        image.DecodePixelHeight = 64;
        image.UriSource = new Uri("pack://application:,,,/Resources/unknown_pack.png");
        image.EndInit();

        if (image.CanFreeze) image.Freeze();

        return image;
    }

    private ProjectTileModel? GetContextModel(object sender)
    {
        return sender is not MenuItem {
            Parent: ContextMenu {
                PlacementTarget: Tile {
                    DataContext: ProjectTileModel model,
                }
            }
        } ? null : model;
    }

    private async void OnContextMenuOpenClick(object sender, RoutedEventArgs e)
    {
        var model = GetContextModel(sender);
        if (model?.Filename == null) return;

        await OpenProjectAsync(model.Filename);
    }

    private async void OnContextMenuRemoveClick(object sender, RoutedEventArgs e)
    {
        var projectModel = GetContextModel(sender);
        if (projectModel == null) return;

        Model.Data.Tiles.Remove(projectModel);

        if (projectModel.Filename != null)
            await Model.Data.RemoveAsync(projectModel.Filename);
    }
}

public class TileClickedEventArgs
{
    public string? Filename {get; set;}
}
