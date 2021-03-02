using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal interface IPublishLocationManager
    {
        Task<LocationDataModel[]> LoadAsync(CancellationToken token = default);
        Task SaveAsync(IEnumerable<LocationDataModel> locations, CancellationToken token = default);
    }

    internal class PublishLocationManager : IPublishLocationManager
    {
        private const string FileName = "PublishLocations.txt";

        private readonly IAppDataHelper appData;


        public PublishLocationManager(IAppDataHelper appData)
        {
            this.appData = appData;
        }

        public Task<LocationDataModel[]> LoadAsync(CancellationToken token = default)
        {
            return appData.ReadJsonAsync<LocationDataModel[]>(FileName, token);
        }

        public Task SaveAsync(IEnumerable<LocationDataModel> locations, CancellationToken token = default)
        {
            return appData.WriteJsonAsync(FileName, locations, token);
        }
    }
}
