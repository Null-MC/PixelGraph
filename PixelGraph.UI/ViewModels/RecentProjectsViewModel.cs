using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Projects;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO;
using PixelGraph.UI.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PixelGraph.UI.ViewModels;

public class RecentProjectsModel(
    ILogger<RecentProjectsModel> logger,
    IRecentPathManager recentMgr)
{
    private readonly ILogger<RecentProjectsModel>? logger = logger;
    private readonly IRecentPathManager? recentMgr = recentMgr;
    private readonly ProjectSerializer serializer = new();

    //public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ProjectTileModel> Tiles {get;} = [];
    public ImageSource? MissingImage {get; set;}


    public async Task LoadAsync()
    {
        ArgumentNullException.ThrowIfNull(recentMgr);

        await recentMgr.LoadAsync();
    }

    public Task AppendProjectAsync(string filename, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(recentMgr);

        recentMgr.Insert(filename);
        return recentMgr.SaveAsync(token);
    }

    public async Task RemoveAsync(string filename, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(recentMgr);

        recentMgr.Remove(filename);
        await recentMgr.SaveAsync(token);
    }

    public async Task UpdateModelListAsync(Dispatcher dispatcher, CancellationToken token = default)
    {
        await dispatcher.BeginInvoke(() => Tiles.Clear());

        await foreach (var (filename, project) in LoadProjectsAsync().WithCancellation(token)) {
            var projectPath = Path.GetDirectoryName(filename);
            if (string.IsNullOrEmpty(projectPath)) {
                logger?.LogWarning("Skipping project with empty/missing path!");
                continue;
            }

            var tile = new ProjectTileModel {
                Filename = filename,
                Name = project.Name ?? "<missing>",
                Description = project.Description,
            };

            try {
                tile.Icon = FindPackIcon(projectPath);
            }
            catch (Exception error) {
                logger?.LogError(error, "Failed to load icon for project '{filename}'!", filename);
            }

            await dispatcher.BeginInvoke(() => Tiles.Add(tile));
        }
    }

    private ImageSource? FindPackIcon(string projectPath)
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
        ArgumentNullException.ThrowIfNull(recentMgr);

        var removeItems = new List<string>();

        foreach (var filename in recentMgr.Items) {
            ProjectData project;
            try {
                project = await serializer.LoadAsync(filename);
            }
            catch (Exception error) {
                logger?.LogError(error, "Failed to load data for recent project '{filename}'!", filename);
                removeItems.Add(filename);
                continue;
            }

            yield return (filename, project);
        }

        foreach (var filename in removeItems)
            recentMgr.Remove(filename);
    }

    //protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}

    //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    //{
    //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    //    field = value;
    //    OnPropertyChanged(propertyName);
    //    return true;
    //}
}

public class RecentProjectsViewModel : ModelBase
{
    public RecentProjectsModel? Data {get; private set;}


    public void Initialize(IServiceProvider provider)
    {
        Data = provider.GetRequiredService<RecentProjectsModel>();
        OnPropertyChanged(nameof(Data));
    }
}
