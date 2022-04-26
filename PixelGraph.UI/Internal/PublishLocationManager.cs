using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal interface IPublishLocationManager
    {
        string SelectedLocation {get; set;}

        LocationDataModel[] GetLocations();
        void SetLocations(IEnumerable<LocationDataModel> locations);
        Task LoadAsync(CancellationToken token = default);
        Task SaveAsync(CancellationToken token = default);
    }

    internal class PublishLocationManager : IPublishLocationManager, IDisposable
    {
        private const string FileName = "PublishLocations.txt";

        private readonly IAppDataUtility appData;
        private readonly ReaderWriterLockSlim _lock;
        private LocationDataModel[] _locations;

        public string SelectedLocation {get; set;}


        public PublishLocationManager(
            IAppSettings appSettings,
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

        public LocationDataModel[] GetLocations()
        {
            _lock.EnterReadLock();

            try {
                return _locations;
            }
            finally {
                _lock.ExitReadLock();
            }
        }

        public void SetLocations(IEnumerable<LocationDataModel> locations)
        {
            _lock.EnterWriteLock();

            try {
                _locations = locations as LocationDataModel[] ?? locations?.ToArray();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        public async Task LoadAsync(CancellationToken token = default)
        {
            var locations = await appData.ReadJsonAsync<LocationDataModel[]>(FileName, token);
            SetLocations(locations);
        }

        public Task SaveAsync(CancellationToken token = default)
        {
            var locations = GetLocations();
            return appData.WriteJsonAsync(FileName, locations, token);
        }
    }
}
