using HelixToolkit.SharpDX.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models.Scene;
using PixelGraph.UI.ViewData;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixelGraph.UI.Controls
{
    public partial class ScenePropertiesControl
    {
        private IAppSettings settings;
        private bool isInitializing;

        public ScenePropertiesModel SceneProperties {
            get => (ScenePropertiesModel)GetValue(ScenePropertiesProperty);
            set => SetValue(ScenePropertiesProperty, value);
        }


        public ScenePropertiesControl()
        {
            isInitializing = true;

            InitializeComponent();
        }

        public void Initialize(IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILogger<ScenePropertiesControl>>();
            settings = provider.GetRequiredService<IAppSettings>();

            SceneProperties.AppSettingsChanged += OnSceneAppSettingsChanged;

            try {
                if (!string.IsNullOrWhiteSpace(settings.Data.RenderPreview.PomType))
                    SceneProperties.PomType = PomTypeValues.ByName(settings.Data.RenderPreview.PomType);

                if (settings.Data.RenderPreview.EnableAtmosphere.HasValue)
                    SceneProperties.EnableAtmosphere = settings.Data.RenderPreview.EnableAtmosphere.Value;

                if (!string.IsNullOrWhiteSpace(settings.Data.RenderPreview.IblFilename))
                    SceneProperties.ErpFilename = settings.Data.RenderPreview.IblFilename;

                if (settings.Data.RenderPreview.IblIntensity.HasValue)
                    SceneProperties.ErpIntensity = settings.Data.RenderPreview.IblIntensity.Value;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to initialize control!");
            }
            finally {
                isInitializing = false;
            }
        }

        private async Task LoadIblTextureAsync()
        {
            var filename = SceneProperties.ErpFilename;
            var texture = await Task.Run(() => TextureModel.Create(filename));
            await Dispatcher.BeginInvoke(() => SceneProperties.EquirectangularMap = texture);
        }

        private async void OnErpFileBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog {
                Title = "Import Zip Archive",
                Filter = "Supported Images|*.bmp;*.png;*.tga;*.gif;*.jpg;*.jpeg;*.webp|All Files|*.*",
                CheckFileExists = true,
            };

            var window = Window.GetWindow(this);
            if (dialog.ShowDialog(window) != true) return;

            SceneProperties.ErpFilename = dialog.FileName;
            await LoadIblTextureAsync();
        }

        private void OnErpFilenamePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) {
                SceneProperties.EquirectangularMap = null;
                SceneProperties.ErpFilename = null;
            }
        }

        private void OnErpRemoveClick(object sender, RoutedEventArgs e)
        {
            SceneProperties.EquirectangularMap = null;
            SceneProperties.ErpFilename = null;
        }

        private void OnSceneAppSettingsChanged(object sender, EventArgs e)
        {
            if (isInitializing) return;

            settings.Data.RenderPreview.PomType = SceneProperties.PomType?.Name;
            settings.Data.RenderPreview.EnableAtmosphere = SceneProperties.EnableAtmosphere;
            settings.Data.RenderPreview.IblFilename = SceneProperties.ErpFilename;
            settings.Data.RenderPreview.IblIntensity = SceneProperties.ErpIntensity;

            Dispatcher.Invoke(() => settings.SaveAsync(), DispatcherPriority.Background);
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SceneProperties.ErpFilename))
                await LoadIblTextureAsync();
        }

        //private async void OnPomTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    settings.Data.RenderPreview.PomType = SceneProperties.PomType?.Name;
        //    await settings.SaveAsync();
        //}

        public static readonly DependencyProperty ScenePropertiesProperty = DependencyProperty
            .Register(nameof(SceneProperties), typeof(ScenePropertiesModel), typeof(ScenePropertiesControl));
    }
}
