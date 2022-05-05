using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Projects;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PixelGraph.UI.ViewModels
{
    public class RecentProjectsViewModel : ModelBase
    {
        private readonly ProjectSerializer serializer;
        private ILogger<RecentProjectsViewModel> logger;
        private IRecentPathManager recentMgr;

        public ImageSource MissingImage {get; set;}
        public ObservableCollection<ProjectTileModel> Tiles {get;}


        public RecentProjectsViewModel()
        {
            Tiles = new ObservableCollection<ProjectTileModel>();
            serializer = new ProjectSerializer();
        }

        public void Initialize(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<RecentProjectsViewModel>>();
            recentMgr = provider.GetRequiredService<IRecentPathManager>();
        }

        public async Task LoadAsync()
        {
            await recentMgr.LoadAsync();
        }

        public Task AppendProjectAsync(string filename, CancellationToken token = default)
        {
            recentMgr.Insert(filename);
            return recentMgr.SaveAsync(token);
        }

        public async Task RemoveAsync(string filename, CancellationToken token = default)
        {
            recentMgr.Remove(filename);
            await recentMgr.SaveAsync(token);
        }

        public async Task UpdateModelListAsync(Dispatcher dispatcher, CancellationToken token = default)
        {
            await dispatcher.BeginInvoke(() => Tiles.Clear());

            await foreach (var (filename, project) in LoadProjectsAsync().WithCancellation(token)) {
                var projectPath = Path.GetDirectoryName(filename);

                var tile = new ProjectTileModel {
                    Filename = filename,
                    Name = project.Name ?? "<missing>",
                    Description = project.Description,
                };

                try {
                    tile.Icon = FindPackIcon(projectPath);
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load icon for project '{filename}'!", filename);
                }

                await dispatcher.BeginInvoke(() => Tiles.Add(tile));
            }
        }

        private ImageSource FindPackIcon(string projectPath)
        {
            foreach (var file in Directory.GetFiles(projectPath, "pack.*", SearchOption.TopDirectoryOnly)) {
                var ext = Path.GetExtension(file);
                if (!ImageExtensions.Supports(ext)) continue;

                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(file, UriKind.Absolute);
                image.EndInit();

                if (image.CanFreeze) image.Freeze();
                return image;
            }

            return MissingImage;
        }

        private async IAsyncEnumerable<(string filename, ProjectData project)> LoadProjectsAsync()
        {
            foreach (var filename in recentMgr.Items) {
                ProjectData project;
                try {
                    project = await serializer.LoadAsync(filename);
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load data for recent project '{filename}'!", filename);
                    continue;
                }

                yield return (filename, project);
            }
        }
    }
}
