using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Windows;
using Serilog;
using System;
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
            builder.Services.AddSingleton<ITextureEditUtility, TextureEditUtility>();

            builder.Services.AddTransient<IServiceBuilder, ServiceBuilder>();
            builder.Services.AddTransient<ITexturePreviewBuilder, TexturePreviewBuilder>();
            builder.Services.AddTransient<IAppDataUtility, AppDataUtility>();
            builder.Services.AddTransient<IThemeHelper, ThemeHelper>();

            builder.Services.AddTransient<SettingsWindow>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = LocalLogFile.FileLogger;
            Log.Information("Application Started.");
            provider = builder.Build();

            try {
                var settings = provider.GetRequiredService<IAppSettings>();
                settings.Load();
            }
            catch (Exception error) {
                Log.Fatal(error, "Failed to load application settings!");
                throw;
            }

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
