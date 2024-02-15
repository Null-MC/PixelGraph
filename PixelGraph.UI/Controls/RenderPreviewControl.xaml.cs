using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Models.Scene;
using PixelGraph.UI.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Controls;

public partial class RenderPreviewControl
{
    private ILogger<RenderPreviewControl>? logger;

    public event EventHandler? RefreshClick;
    public event EventHandler<ShaderCompileErrorEventArgs>? ShaderCompileErrors;

    public RenderPreviewViewModel? ViewModel {get; private set;}

    public ScenePropertiesModel SceneProperties {
        get => (ScenePropertiesModel)GetValue(ScenePropertiesProperty);
        set => SetValue(ScenePropertiesProperty, value);
    }

    public RenderPropertiesModel RenderProperties {
        get => (RenderPropertiesModel)GetValue(RenderPropertiesProperty);
        set => SetValue(RenderPropertiesProperty, value);
    }

    public string? FrameRateText {
        get => (string?)GetValue(FrameRateTextProperty);
    }

    public string? DeviceNameText {
        get => (string?)GetValue(DeviceNameTextProperty);
        set => SetValue(DeviceNameTextProperty, value);
    }


    public RenderPreviewControl()
    {
        InitializeComponent();

        // ERROR: HOW TF DO I SET THE BUFFER FORMAT!?!?!?!
        //viewport3D.RenderHost.RenderBuffer.Format = Format.R16G16B16A16_Float;
        //viewport3D.RenderContext.RenderHost.GetHashCode();
    }

    public async Task InitializeAsync(IServiceProvider provider, CancellationToken token = default)
    {
        logger = provider.GetRequiredService<ILogger<RenderPreviewControl>>();

        RenderProperties.DielectricBrdfLutMap = DielectricBdrfLutMapSource;
        RenderProperties.DynamicSkyCubeSource = DynamicSkyCubeSource;
        RenderProperties.ErpCubeSource = EquirectangularCubeMapSource;
        RenderProperties.IrradianceCube = IrradianceCubeMapSource;

        ViewModel = new RenderPreviewViewModel(provider) {
            SceneProperties = SceneProperties,
            RenderProperties = RenderProperties,
        };

        ViewModel.RenderModelChanged += OnRenderModelChanged;
        ViewModel.ShaderCompileErrors += OnViewModelShaderCompileErrors;

        ViewModel.Initialize();

        //try {
        //    DeviceNameText = ViewModel.GetDeviceName();
        //}
        //catch (Exception error) {
        //    logger.LogError(error, "Failed to retrieve display device description!");
        //}

        ViewModel.ShaderCompileErrors += OnShaderCompileErrors;

        try {
            await Task.Run(() => ViewModel.ReloadShaders(), token);
        }
        catch (Exception error) {
            logger.LogError(error, "Failed to load 3D preview content!");
            await Dispatcher.BeginInvoke(() => ShowWindowError($"Failed to load 3D preview content! {error.UnfoldMessageString()}"));
        }
            
        await Dispatcher.BeginInvoke(() => {
            try {
                //viewport3D.RenderHost.RenderConfiguration.EnableVSync = false;

                ViewModel.Prepare();

                try {
                    DeviceNameText = ViewModel.GetDeviceName();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to retrieve display device description!");
                }

                Model.UpdateSunPosition();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to initialize 3D preview!");
                ShowWindowError($"Failed to initialize 3D preview! {error.UnfoldMessageString()}");
            }
        });
    }

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

    private async void OnRenderModeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!Model.IsLoaded) return;

        OnRefreshClick();
        await Task.Run(() => ViewModel?.SaveRenderStateAsync());
    }

    private void OnPreviewRefreshClick(object? sender, RoutedEventArgs e)
    {
        OnRefreshClick();
    }

    private void OnRefreshClick()
    {
        RefreshClick?.Invoke(this, EventArgs.Empty);
    }

    private void OnViewModelShaderCompileErrors(object? sender, ShaderCompileErrorEventArgs e)
    {
        if (e.Errors != null) ThrowShaderCompileErrors(e.Errors);
    }

    private async void OnShaderCompileErrors(object? sender, ShaderCompileErrorEventArgs e)
    {
        var message = new StringBuilder("Failed to compile shaders!");

        if (e.Errors != null) {
            foreach (var error in e.Errors) {
                message.AppendLine();
                message.Append(error.Message);
            }
        }

        await Dispatcher.BeginInvoke(() => ShowWindowError(message.ToString()));
    }

    private void OnRenderModelChanged(object? sender, EventArgs e)
    {
        OnRefreshClick();

        // TODO: replace this with UpdateModelParts()
        // need to rebind existing materials for this to help
    }

    private void OnControlPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.I && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            Model.RenderProperties.ShowIrradiance = true;
    }

    private void OnControlPreviewKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.I && Model.RenderProperties.ShowIrradiance)
            Model.RenderProperties.ShowIrradiance = false;
    }

    private void ShowWindowError(string message)
    {
        var window = Window.GetWindow(this);
        if (window == null) return;

        MessageBox.Show(window, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    //private void OnEnvironmentChanged(object sender, EventArgs e)
    //{
    //    // TODO: this really only needs to refresh the material cubeMap bindings
    //    //OnRefreshClick();

    //    tabMgr.
    //}

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
}