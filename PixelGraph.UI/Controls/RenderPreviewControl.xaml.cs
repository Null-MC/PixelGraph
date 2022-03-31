using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Rendering.Shaders;
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

namespace PixelGraph.UI.Controls
{
    public partial class RenderPreviewControl
    {
        private ILogger<RenderPreviewControl> logger;

        public event EventHandler RefreshClick;
        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public RenderPreviewViewModel ViewModel {get; private set;}

        public ScenePropertiesModel SceneProperties {
            get => (ScenePropertiesModel)GetValue(ScenePropertiesProperty);
            set => SetValue(ScenePropertiesProperty, value);
        }

        public RenderPropertiesModel RenderProperties {
            get => (RenderPropertiesModel)GetValue(RenderPropertiesProperty);
            set => SetValue(RenderPropertiesProperty, value);
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

            // ERROR: HOW TF DO I SET THE BUFFER FORMAT!?!?!?!
            //viewport3D.RenderHost.RenderBuffer.Format = Format.R16G16B16A16_Float;
        }

        public async Task InitializeAsync(IServiceProvider provider, CancellationToken token = default)
        {
            logger = provider.GetRequiredService<ILogger<RenderPreviewControl>>();

            RenderProperties.EnvironmentCube = EnvironmentCubeMapSource;
            RenderProperties.IrradianceCube = IrradianceCubeMapSource;

            ViewModel = new RenderPreviewViewModel(provider) {
                SceneProperties = SceneProperties,
                RenderProperties = RenderProperties,
            };

            //SceneProperties.SceneChanged += OnSceneSunChanged;
            ViewModel.RenderModelChanged += OnRenderModelChanged;
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

        //private void OnSceneChanged(object sender, EventArgs e)
        //{
        //    ViewModel.UpdateSun();
        //}

        private void OnRenderModelChanged(object sender, EventArgs e)
        {
            OnRefreshClick();

            //viewport3D.ZoomExtents();
        }

        private void ShowWindowError(string message)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;

            MessageBox.Show(window, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnSceneChanged(object sender, EventArgs e)
        {
            ViewModel.UpdateSun();
            
            // ERROR: calling this causes the materials to completely reload while adjusting TOD
            //OnRefreshClick();
        }

        private static void OnScenePropertiesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RenderPreviewControl control)
                control.Model.SceneProperties = (ScenePropertiesModel)e.NewValue;
        }

        private static void OnRenderPropertiesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RenderPreviewControl control)
                control.Model.RenderProperties = (RenderPropertiesModel)e.NewValue;
        }

        public static readonly DependencyProperty ScenePropertiesProperty = DependencyProperty
            .Register(nameof(SceneProperties), typeof(ScenePropertiesModel), typeof(RenderPreviewControl),
                new UIPropertyMetadata(null, OnScenePropertiesChangedCallback));

        public static readonly DependencyProperty RenderPropertiesProperty = DependencyProperty
            .Register(nameof(RenderProperties), typeof(RenderPropertiesModel), typeof(RenderPreviewControl),
                new UIPropertyMetadata(null, OnRenderPropertiesChangedCallback));

        public static readonly DependencyProperty FrameRateTextProperty = DependencyProperty
            .Register(nameof(FrameRateText), typeof(string), typeof(RenderPreviewControl));

        public static readonly DependencyProperty DeviceNameTextProperty = DependencyProperty
            .Register(nameof(DeviceNameText), typeof(string), typeof(RenderPreviewControl));

        //private void OnEnableTilingChecked(object sender, RoutedEventArgs e)
        //{
        //    OnRefreshClick();
        //}
    }
}
