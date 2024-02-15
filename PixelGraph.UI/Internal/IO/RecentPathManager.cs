using PixelGraph.UI.Internal.Settings;

namespace PixelGraph.UI.Internal.IO;

public interface IRecentPathManager
{
    IEnumerable<string> Items {get;}

    Task LoadAsync(CancellationToken token = default);
    Task SaveAsync(CancellationToken token = default);

    void Insert(string path);
    void Remove(string path);
}

internal class RecentPathManager(
    IAppSettingsManager appSettingsMgr,
    IAppDataUtility appData)
    : IRecentPathManager
{
    private const string FileName = "Recent.txt";

    private readonly List<string> _items = new();

    public IEnumerable<string> Items => _items;


    public async Task LoadAsync(CancellationToken token = default)
    {
        _items.Clear();

        var lines = await appData.ReadLinesAsync(FileName, token);
        //if (lines == null) return;

        _items.AddRange(lines);
    }

    public Task SaveAsync(CancellationToken token = default)
    {
        return appData.WriteLinesAsync(FileName, _items, token);
    }

    public void Insert(string path)
    {
        _items.Remove(path);
        _items.Insert(0, path);

        var recentMaxCount = appSettingsMgr.Data.MaxRecentProjects 
            ?? AppSettingsDataModel.DefaultMaxRecentProjects;

        for (var i = _items.Count - 1; i >= recentMaxCount; i--)
            _items.RemoveAt(i);
    }

    public void Remove(string path)
    {
        _items.Remove(path);
    }
}
