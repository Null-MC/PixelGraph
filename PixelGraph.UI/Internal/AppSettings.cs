using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal interface IAppSettings
    {
        IAppConfiguration Configuration {get;}

        Task LoadAsync(CancellationToken token = default);
        Task SaveAsync(CancellationToken token = default);
    }

    internal class AppSettings : IAppSettings
    {
        private readonly string appDataPath;
        private readonly string filename;

        public IAppConfiguration Configuration {get;}


        public AppSettings(IAppConfiguration configuration)
        {
            Configuration = configuration;

            var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = PathEx.Join(dataPath, "PixelGraph");
            filename = PathEx.Join(appDataPath, "settings.json");
        }

        public async Task LoadAsync(CancellationToken token = default)
        {
            if (!File.Exists(filename)) return;

            await using var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var data = await JToken.ReadFromAsync(jsonReader, token);

            Configuration.PublishCloseOnComplete = data.Value<bool>(nameof(Configuration.PublishCloseOnComplete));
        }

        public async Task SaveAsync(CancellationToken token = default)
        {
            var data = new JObject {
                [nameof(Configuration.PublishCloseOnComplete)] = Configuration.PublishCloseOnComplete,
            };

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer);
            await data.WriteToAsync(jsonWriter, token);
        }
    }
}
