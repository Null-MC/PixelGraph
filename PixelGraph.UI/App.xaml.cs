using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using PixelGraph.UI.Windows;
using System.Windows;

namespace PixelGraph.UI
{
    public partial class App
    {
        private readonly ServiceBuilder builder;
        private ServiceProvider provider;


        public App()
        {
            builder = new ServiceBuilder();
            builder.AddFileInput();
            builder.AddFileOutput();

            //builder.Services.AddSingleton<IAppConfiguration, AppConfiguration>();
            builder.Services.AddSingleton<IAppSettings, AppSettings>();
            builder.Services.AddSingleton<IRecentPathManager, RecentPathManager>();
            builder.Services.AddSingleton<IPublishLocationManager, PublishLocationManager>();
            builder.Services.AddSingleton<IContentTreeReader, ContentTreeReader>();

            builder.Services.AddTransient<IServiceBuilder, ServiceBuilder>();
            builder.Services.AddTransient<IAppDataHelper, AppDataHelper>();
            builder.Services.AddTransient<ITexturePreviewBuilder, TexturePreviewBuilder>();

            builder.Services.AddTransient<MainWindowVM>();
            builder.Services.AddTransient<SettingsWindowVM>();
            builder.Services.AddTransient<PublishWindowVM>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            provider = builder.Build();

            var mainWindow = new MainWindow(provider);
            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            provider?.Dispose();
        }
    }
}
