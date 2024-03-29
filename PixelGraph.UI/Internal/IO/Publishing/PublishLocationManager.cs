﻿using PixelGraph.Common.IO;
using PixelGraph.UI.Internal.Settings;
using System.IO;

namespace PixelGraph.UI.Internal.IO.Publishing;

internal interface IPublishLocationManager
{
    string? SelectedLocation {get; set;}

    PublishLocation[]? GetLocations();
    void SetLocations(IEnumerable<PublishLocation>? locations);
    Task LoadAsync(CancellationToken token = default);
    Task SaveAsync(CancellationToken token = default);
}

internal class PublishLocationManager(
    IAppSettingsManager appSettings,
    IAppDataUtility appData)
    : IPublishLocationManager, IDisposable
{
    private const string FileName = "PublishLocations.json";

    private readonly ReaderWriterLockSlim _lock = new();
    private PublishLocation[]? _locations;

    public string? SelectedLocation {get; set;} = appSettings.Data.SelectedPublishLocation;


    public void Dispose()
    {
        _lock.Dispose();
    }

    public PublishLocation[]? GetLocations()
    {
        _lock.EnterReadLock();

        try {
            return _locations;
        }
        finally {
            _lock.ExitReadLock();
        }
    }

    public void SetLocations(IEnumerable<PublishLocation>? locations)
    {
        _lock.EnterWriteLock();

        try {
            _locations = locations as PublishLocation[] ?? locations?.ToArray();
        }
        finally {
            _lock.ExitWriteLock();
        }
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        // Patch for renaming old txt files to new json filename
        var txtFile = Path.Join(AppDataHelper.AppDataPath, "PublishLocations.txt");
        var jsonFile = Path.Join(AppDataHelper.AppDataPath, FileName);
        if (File.Exists(txtFile) && !File.Exists(jsonFile)) File.Move(txtFile, jsonFile);

        var locations = await appData.ReadJsonAsync<PublishLocation[]>(FileName, token);
        SetLocations(locations);
    }

    public Task SaveAsync(CancellationToken token = default)
    {
        var locations = GetLocations() ?? Array.Empty<PublishLocation>();
        return appData.WriteJsonAsync(FileName, locations, token);
    }
}
