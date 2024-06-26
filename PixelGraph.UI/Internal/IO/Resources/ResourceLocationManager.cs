﻿namespace PixelGraph.UI.Internal.IO.Resources;

public interface IResourceLocationManager
{
    ResourceLocation[]? GetLocations();
    void SetLocations(IEnumerable<ResourceLocation> locations);
    Task LoadAsync(CancellationToken token = default);
    Task SaveAsync(CancellationToken token = default);
}

internal class ResourceLocationManager(IAppDataUtility appData) : IResourceLocationManager, IDisposable
{
    private const string FileName = "Resources.json";

    private readonly ReaderWriterLockSlim _lock = new();
    private ResourceLocation[]? _locations;


    public void Dispose()
    {
        _lock.Dispose();
    }

    public ResourceLocation[]? GetLocations()
    {
        _lock.EnterReadLock();

        try {
            return _locations;
        }
        finally {
            _lock.ExitReadLock();
        }
    }

    public void SetLocations(IEnumerable<ResourceLocation>? locations)
    {
        _lock.EnterWriteLock();

        try {
            _locations = locations as ResourceLocation[] ?? locations?.ToArray();
        }
        finally {
            _lock.ExitWriteLock();
        }
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        var locations = await appData.ReadJsonAsync<ResourceLocation[]>(FileName, token);
        SetLocations(locations);
    }

    public Task SaveAsync(CancellationToken token = default)
    {
        var locations = GetLocations() ?? Array.Empty<ResourceLocation>();
        return appData.WriteJsonAsync(FileName, locations, token);
    }
}