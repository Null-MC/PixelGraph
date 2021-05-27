using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using PixelGraph.UI.Windows;
using Serilog;
using System.Windows;
using System.Windows.Threading;

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

            builder.Services.AddSingleton<IAppSettings, AppSettings>();
            builder.Services.AddSingleton<IRecentPathManager, RecentPathManager>();
            builder.Services.AddSingleton<IPublishLocationManager, PublishLocationManager>();
            builder.Services.AddSingleton<IContentTreeReader, ContentTreeReader>();

            builder.Services.AddTransient<IServiceBuilder, ServiceBuilder>();
            builder.Services.AddTransient<ITexturePreviewBuilder, TexturePreviewBuilder>();
            builder.Services.AddTransient<IAppDataUtility, AppDataUtility>();
            builder.Services.AddTransient<ITextureEditUtility, TextureEditUtility>();

            builder.Services.AddTransient<MainWindowVM>();
            builder.Services.AddTransient<SettingsWindowVM>();
            builder.Services.AddTransient<PublishWindowVM>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = LocalLogFile.FileLogger;
            Log.Information("Application Started.");

            provider = builder.Build();

            var mainWindow = new MainWindow(provider);
            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            provider?.Dispose();
            Log.CloseAndFlush();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "An unhandled exception occurred!");
        }
    }
}
