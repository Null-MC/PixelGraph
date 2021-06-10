using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Settings;
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
        private MainWindow window;


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
            builder.Services.AddSingleton<IShaderByteCodeManager, CustomShaderManager>();

            builder.Services.AddTransient<IServiceBuilder, ServiceBuilder>();
            builder.Services.AddTransient<ILayerPreviewBuilder, LayerPreviewBuilder>();
            builder.Services.AddTransient<IRenderDiffusePreviewBuilder, RenderDiffusePreviewBuilder>();
            builder.Services.AddTransient<IRenderPbrPreviewBuilder, RenderPbrPreviewBuilder>();
            builder.Services.AddTransient<IAppDataUtility, AppDataUtility>();
            builder.Services.AddTransient<IThemeHelper, ThemeHelper>();
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

            window = new MainWindow(provider);
            window.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            try {
                provider?.Dispose();
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "An unhandled exception occurred!");
        }
    }
}
