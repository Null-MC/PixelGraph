using PixelGraph.Common.IO;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.IO.Publishing
{
    internal interface IPublishLocationManager
    {
        string SelectedLocation {get; set;}

        PublishLocation[] GetLocations();
        void SetLocations(IEnumerable<PublishLocation> locations);
        Task LoadAsync(CancellationToken token = default);
        Task SaveAsync(CancellationToken token = default);
    }

    internal class PublishLocationManager : IPublishLocationManager, IDisposable
    {
        private const string FileName = "PublishLocations.json";

        private readonly IAppDataUtility appData;
        private readonly ReaderWriterLockSlim _lock;
        private PublishLocation[] _locations;

        public string SelectedLocation {get; set;}


        public PublishLocationManager(
            IAppSettingsManager appSettings,
            IAppDataUtility appData)
        {
            this.appData = appData;

            _lock = new ReaderWriterLockSlim();
            SelectedLocation = appSettings.Data.SelectedPublishLocation;
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }

        public PublishLocation[] GetLocations()
        {
            _lock.EnterReadLock();

            try {
                return _locations;
            }
            finally {
                _lock.ExitReadLock();
            }
        }

        public void SetLocations(IEnumerable<PublishLocation> locations)
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
            var locations = GetLocations();
            return appData.WriteJsonAsync(FileName, locations, token);
        }
    }
}
