using MahApps.Metro.Controls;
using PixelGraph.UI.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PixelGraph.UI.Controls
{
    public class TileClickedEventArgs
    {
        public string Filename {get; set;}
    }

    public partial class RecentProjectsControl
    {
        private ILogger<RecentProjectsControl> logger;

        public event EventHandler<TileClickedEventArgs> TileClicked;


        public RecentProjectsControl()
        {
            InitializeComponent();
        }

        public void Initialize(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<RecentProjectsControl>>();

            Model.MissingImage = CreateMissingImage();

            Model.Initialize(provider);
        }

        public async Task AppendAsync(string filename, CancellationToken token = default)
        {
            await Model.AppendProjectAsync(filename, token);
            await Model.UpdateModelListAsync(Dispatcher, token);
        }

        private async Task OpenProjectAsync(string filename)
        {
            if (!File.Exists(filename)) {
                var window = Window.GetWindow(this);
                if (window != null) MessageBox.Show(window, "The selected project file could not be found!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                await Model.RemoveAsync(filename);
                await Model.UpdateModelListAsync(Dispatcher);
                return;
            }

            TileClicked?.Invoke(this, new TileClickedEventArgs {Filename = filename});
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            try {
                await Task.Run(() => Model.LoadAsync());
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load recent projects!");
                var window = Window.GetWindow(this);
                if (window != null) await Dispatcher.BeginInvoke(() => MessageBox.Show(window, "Failed to load recent project list!", "Error"));
            }

            await Task.Run(() => Model.UpdateModelListAsync(Dispatcher));
        }

        private void OnTileClick(object sender, RoutedEventArgs e)
        {
            if ((e.Source as Tile)?.DataContext is not ProjectTileModel project) return;

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

        private ProjectTileModel GetContextModel(object sender)
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
            await OpenProjectAsync(model.Filename);
        }

        private async void OnContextMenuRemoveClick(object sender, RoutedEventArgs e)
        {
            var projectModel = GetContextModel(sender);
            Model.Tiles.Remove(projectModel);
            await Model.RemoveAsync(projectModel.Filename);
        }
    }
}
