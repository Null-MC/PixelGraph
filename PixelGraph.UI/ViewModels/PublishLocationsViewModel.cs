using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishLocationsViewModel
    {
        private readonly IPublishLocationManager locationMgr;

        public PublishLocationsModel Model {get; set;}


        public PublishLocationsViewModel(IServiceProvider provider)
        {
            locationMgr = provider.GetRequiredService<IPublishLocationManager>();
        }

        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            var rows = Model.Locations.Select(x => x.DataSource);
            await locationMgr.SaveAsync(rows, token);
        }
    }
}
