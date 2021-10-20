using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Models.Scene;
using PixelGraph.UI.ViewModels;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PixelGraph.Rendering.Shaders;

namespace PixelGraph.UI.Controls
{
    public partial class RenderPreviewControl
    {
        private ILogger<RenderPreviewControl> logger;

        public event EventHandler RefreshClick;
        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public RenderPreviewViewModel ViewModel {get; private set;}

        public ScenePropertiesModel SceneModel {
            get => (ScenePropertiesModel)GetValue(SceneModelProperty);
            set => SetValue(SceneModelProperty, value);
        }

        public string FrameRateText {
            get => (string)GetValue(FrameRateTextProperty);
            //set => SetValue(FrameRateTextProperty, value);
        }

        public string DeviceNameText {
            get => (string)GetValue(DeviceNameTextProperty);
            set => SetValue(DeviceNameTextProperty, value);
        }


        public RenderPreviewControl()
        {
            InitializeComponent();

            Model.EnvironmentCube = EnvironmentCubeMapSource;
            Model.IrradianceCube = IrradianceCubeMapSource;
        }

        public async Task InitializeAsync(IServiceProvider provider, CancellationToken token = default)
        {
            logger = provider.GetRequiredService<ILogger<RenderPreviewControl>>();

            ViewModel = new RenderPreviewViewModel(provider) {
                SceneModel = SceneModel,
                RenderModel = Model,
            };

            SceneModel.SunChanged += OnSceneSunChanged;
            ViewModel.ShaderCompileErrors += OnViewModelShaderCompileErrors;

            ViewModel.Initialize();

            try {
                DeviceNameText = ViewModel.GetDeviceName();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to retrieve display device description!");
            }

            ViewModel.ShaderCompileErrors += OnShaderCompileErrors;

            try {
                await Task.Run(() => ViewModel.LoadContentAsync(), token);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load 3D preview content!");
                await Dispatcher.BeginInvoke(() => ShowWindowError($"Failed to load 3D preview content! {error.UnfoldMessageString()}"));
            }
            
            await Dispatcher.BeginInvoke(() => {
                try {
                    ViewModel.Prepare();

                    try {
                        DeviceNameText = ViewModel.GetDeviceName();
                    }
                    catch (Exception error) {
                        logger.LogError(error, "Failed to retrieve display device description!");
                    }

                    ViewModel.UpdateSun();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to initialize 3D preview!");
                    ShowWindowError($"Failed to initialize 3D preview! {error.UnfoldMessageString()}");
                }
            });
        }

        //public void UpdateModel(MaterialProperties material)
        //{
        //    try {
        //        ViewModel.UpdateModel(material);
        //    }
        //    catch (Exception error) {
        //        logger.LogError(error, "Failed to load model!");
        //        // TODO: show warning message!

        //        var window = Window.GetWindow(this);
        //        if (window != null) MessageBox.Show(window, "Failed to load model!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        public BitmapSource TakeScreenshot()
        {
            return viewport3D.RenderBitmap();
        }

        private void ThrowShaderCompileErrors(ShaderCompileError[] errors)
        {
            var e = new ShaderCompileErrorEventArgs {
                Errors = errors,
            };

            ShaderCompileErrors?.Invoke(this, e);
        }

        private async void OnRenderModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Model.IsLoaded) return;

            await Task.Run(() => ViewModel.SaveRenderStateAsync());
        }

        private void OnPreviewRefreshClick(object sender, RoutedEventArgs e)
        {
            OnRefreshClick();
        }

        private void OnRefreshClick()
        {
            RefreshClick?.Invoke(this, EventArgs.Empty);
        }

        private void OnViewModelShaderCompileErrors(object sender, ShaderCompileErrorEventArgs e)
        {
            ThrowShaderCompileErrors(e.Errors);
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.Dispose();
        }

        private async void OnShaderCompileErrors(object sender, ShaderCompileErrorEventArgs e)
        {
            var message = new StringBuilder("Failed to compile shaders!");

            foreach (var error in e.Errors) {
                message.AppendLine();
                message.Append(error.Message);
            }

            await Dispatcher.BeginInvoke(() => ShowWindowError(message.ToString()));
        }

        //private void OnModelModelChanged(object sender, EventArgs e)
        //{
        //    ViewModel.UpdateModel();
        //}

        private void OnSceneSunChanged(object sender, EventArgs e)
        {
            ViewModel.UpdateSun();
        }

        private void ShowWindowError(string message)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;

            MessageBox.Show(window, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static readonly DependencyProperty SceneModelProperty = DependencyProperty
            .Register(nameof(SceneModel), typeof(ScenePropertiesModel), typeof(RenderPreviewControl));

        public static readonly DependencyProperty FrameRateTextProperty = DependencyProperty
            .Register(nameof(FrameRateText), typeof(string), typeof(RenderPreviewControl));

        public static readonly DependencyProperty DeviceNameTextProperty = DependencyProperty
            .Register(nameof(DeviceNameText), typeof(string), typeof(RenderPreviewControl));
    }
}
