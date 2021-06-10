using PixelGraph.UI.Internal.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Settings
{
    public interface IAppSettings
    {
        AppSettingsDataModel Data {get;}

        void Load();
        //Task LoadAsync(CancellationToken token = default);
        Task SaveAsync(CancellationToken token = default);
    }

    internal class AppSettings : IAppSettings
    {
        private const string FileName = "Settings.json";

        private readonly IAppDataUtility appData;

        public AppSettingsDataModel Data {get; private set;}


        public AppSettings(IAppDataUtility appData)
        {
            this.appData = appData;

            Data = new AppSettingsDataModel();
        }

        public void Load()
        {
            Data = appData.ReadJson<AppSettingsDataModel>(FileName) ?? new AppSettingsDataModel();
        }

        //public async Task LoadAsync(CancellationToken token = default)
        //{
        //    Data = await appData.ReadJsonAsync<SettingsDataModel>(FileName, token) ?? new SettingsDataModel();
        //}

        public Task SaveAsync(CancellationToken token = default)
        {
            if (Data == null) return Task.CompletedTask;
            return appData.WriteJsonAsync(FileName, Data, token);
        }
    }
}
