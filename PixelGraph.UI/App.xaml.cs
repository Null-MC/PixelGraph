using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Models;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Windows;
using Serilog;
using System;
using System.Windows;
using System.Windows.Threading;
using PixelGraph.Common.Extensions;
using Serilog.Events;

#if !NORENDER
using PixelGraph.Rendering.Models;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Helix.Models;
#endif

namespace PixelGraph.UI
{
    public partial class App
    {
        private readonly ServiceCollection services;
        private ServiceProvider provider;
        private MainWindow window;


        public App()
        {
            services = new ServiceCollection();

            services.AddLogging(builder => builder
                .AddSerilog());

            services.AddSingleton<IAppSettings, AppSettings>();
            services.AddSingleton<IProjectContextManager, ProjectContextManager>();
            services.AddSingleton<IRecentPathManager, RecentPathManager>();
            services.AddSingleton<IPublishLocationManager, PublishLocationManager>();
            services.AddSingleton<ITabPreviewManager, TabPreviewManager>();
            services.AddSingleton<MaterialPropertiesCache>();
            services.AddSingleton<TextureEditUtility>();

            services.AddTransient<IServiceBuilder, ServiceBuilder>();
            services.AddTransient<ILayerPreviewBuilder, LayerPreviewBuilder>();
            services.AddTransient<IAppDataUtility, AppDataUtility>();
            services.AddTransient<IThemeHelper, ThemeHelper>();
            services.AddTransient<ModelLoader>();

#if !NORENDER
            services.AddSingleton<IShaderByteCodeManager, CustomShaderManager>();

            services.AddTransient<IRenderDiffusePreviewBuilder, RenderDiffusePreviewBuilder>();
            services.AddTransient<IRenderNormalsPreviewBuilder, RenderNormalsPreviewBuilder>();
            services.AddTransient<IRenderOldPbrPreviewBuilder, RenderOldPbrPreviewBuilder>();
            services.AddTransient<IRenderLabPbrPreviewBuilder, RenderLabPbrPreviewBuilder>();

            services.AddTransient<BlockModelBuilder>();
            services.AddTransient<EntityModelBuilder>();
            services.AddTransient<MultiPartMeshBuilder>();
#endif
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = LocalLogFile.FileLogger;
            Log.Information("Application Started.");
            provider = services.BuildServiceProvider();

            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            var settings = provider.GetRequiredService<IAppSettings>();

            try {
                settings.Load();
            }
            catch (Exception error) {
                Log.Fatal(error, "Failed to load application settings!");
                throw;
            }

            try {
                window = new MainWindow(provider);
                window.Show();
            }
            catch (Exception error) {
                Log.Error(error, "Failed to show Main window!");
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            try {
                provider?.Dispose();
            }
            catch (Exception error) {
                Log.Error(error, "An error occurred while disposing resources!");
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "An unhandled exception occurred!");
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var level = e.IsTerminating ? LogEventLevel.Fatal : LogEventLevel.Error;
            Log.Write(level, e.ExceptionObject as Exception, "An unhandled exception occurred!");
        }
    }
}
