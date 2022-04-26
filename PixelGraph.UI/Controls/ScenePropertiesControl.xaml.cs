using HelixToolkit.SharpDX.Core;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Models.Scene;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixelGraph.UI.Controls
{
    public partial class ScenePropertiesControl
    {
        public ScenePropertiesModel SceneProperties {
            get => (ScenePropertiesModel)GetValue(ScenePropertiesProperty);
            set => SetValue(ScenePropertiesProperty, value);
        }


        public ScenePropertiesControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScenePropertiesProperty = DependencyProperty
            .Register(nameof(SceneProperties), typeof(ScenePropertiesModel), typeof(ScenePropertiesControl));

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
            var texture = await Task.Run(() => TextureModel.Create(dialog.FileName));
            await Dispatcher.BeginInvoke(() => SceneProperties.EquirectangularMap = texture);
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
    }
}
