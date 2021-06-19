using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.Materials;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models;
using SharpDX;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    internal class PreviewViewModel : IDisposable, IAsyncDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly object lockHandle;

        private CancellationTokenSource tokenSource;
        private DiffuseMaterialBuilder builderDiffuse;
        private PbrMaterialBuilder builderPbr;

        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public MainModel Model {get; set;}
        public Dispatcher Dispatcher {get; set;}


        public PreviewViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            appSettings = provider.GetRequiredService<IAppSettings>();
            
            lockHandle = new object();
        }

        public void Dispose()
        {
            builderDiffuse?.Dispose();
            builderPbr?.Dispose();
            tokenSource?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (builderDiffuse != null)
                await builderDiffuse.DisposeAsync();

            if (builderPbr != null)
                await builderPbr.DisposeAsync();

            // ReSharper disable once InconsistentlySynchronizedField
            tokenSource?.Dispose();
        }

        public void Initialize()
        {
            LoadAppSettings();
            ReloadShaders();
            
            Model.Preview.Camera = new PerspectiveCamera();

            //Model.Preview.SunCamera = new PerspectiveCamera { 
            //    UpDirection = new Media3D.Vector3D(1, 0, 0), 
            //    FarPlaneDistance = 5000, 
            //    NearPlaneDistance = 1,
            //    FieldOfView = 45
            //};
            //Model.Preview.SunCamera = new OrthographicCamera { 
            //    UpDirection = new Media3D.Vector3D(1, 0, 0),
            //    FarPlaneDistance = 200, 
            //    NearPlaneDistance = 1,
            //};

            ResetViewport();

            Model.Preview.Model = BuildCube(4);

            builderDiffuse = new DiffuseMaterialBuilder(provider) {
                Model = Model,
            };

            builderPbr = new PbrMaterialBuilder(provider) {
                Model = Model,
            };

            Model.Preview.PropertyChanged += OnPreviewPropertyChanged;
            Model.Preview.EnableRenderChanged += OnEnableRenderChanged;
            Model.Preview.RenderModeChanged += OnRenderModeChanged;
            Model.Preview.RenderSceneChanged += OnRenderSceneChanged;
            Model.Preview.SelectedTagChanged += OnSelectedTagChanged;
        }

        public void LoadAppSettings()
        {
            Model.Preview.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
            Model.Preview.ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.Preview.ParallaxSamplesMax = appSettings.Data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;
            Model.Preview.EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;

            if (appSettings.Data.RenderPreview.SelectedMode != null)
                if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                    Model.Preview.RenderMode = renderMode;
        }

        public void UpdateSun()
        {
            //const float sun_distance = 10f;
            const float sun_overlap = 0.12f;
            const float sun_power = 0.85f;
            const float sun_azimuth = 30f;
            const float sun_roll = 15f;

            //var linearTimeOfDay = Model.Preview.TimeOfDayLinear;
            MinecraftTime.GetSunAngle(sun_azimuth, sun_roll, Model.Preview.TimeOfDayLinear, out var sunDirection);
            Model.Preview.SunDirection = sunDirection;

            var strength = MinecraftTime.GetSunStrength(Model.Preview.TimeOfDayLinear, sun_overlap, sun_power); // * (1f - Wetness * 0.6f);
            //Model.Preview.SunLightColor = new Color4(strength, strength, strength, strength);
            Model.Preview.SunStrength = strength;
        }

        public Task SaveRenderStateAsync(CancellationToken token = default)
        {
            appSettings.Data.RenderPreview.SelectedMode = RenderPreviewMode.GetString(Model.Preview.RenderMode);
            return appSettings.SaveAsync(token);
        }

        private void OnPreviewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PreviewContextModel.TimeOfDay)) UpdateSun();
        }

        public void ReloadShaders()
        {
            var shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            if (!shaderMgr.LoadAll(out var compileErrors))
                OnShaderCompileErrors(compileErrors);

            Model.Preview.EffectsManager?.Dispose();
            Model.Preview.EffectsManager = new CustomEffectsManager(provider);
        }

        public void ResetViewport()
        {
            Model.Preview.Camera.Position = new Media3D.Point3D(10, 10, 10);
            Model.Preview.Camera.LookDirection = new Media3D.Vector3D(-10, -10, -10);
            Model.Preview.Camera.UpDirection = new Media3D.Vector3D(0, 1, 0);

            Model.Preview.PointLightTransform = new Media3D.MatrixTransform3D();
            Model.Preview.PointLightTransform.Transform(new Media3D.Vector3D(5, 7, 5));

            Model.Preview.TimeOfDay = 8_000;
        }

        public async Task ClearAsync()
        {
            await Dispatcher.BeginInvoke(() => Model.Preview.LayerImage = null);

            await builderDiffuse.ClearAllTexturesAsync();
            await builderPbr.ClearAllTexturesAsync();
        }

        public async Task SetFromFileAsync(string filename)
        {
            await Dispatcher.BeginInvoke(() => Model.Preview.IsLoading = true);

            var texImage = new BitmapImage();

            texImage.BeginInit();
            texImage.CacheOption = BitmapCacheOption.OnLoad;
            texImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            texImage.UriSource = new Uri(filename);
            texImage.EndInit();

            texImage.Freeze();

            await Dispatcher.BeginInvoke(() => {
                Model.Preview.LayerImage = texImage;
                Model.Preview.IsLoading = false;
            });
        }

        private CancellationToken StartNewToken(CancellationToken token)
        {
            lock (lockHandle) {
                tokenSource?.Cancel();
                tokenSource?.Dispose();

                tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                return tokenSource.Token;
            }
        }

        public async Task UpdateAsync(bool clear, CancellationToken token = default)
        {
            var mergedToken = StartNewToken(token);
            if (clear) await ClearAsync();

            var hasContent = Model.Material.HasLoaded; // && VM.HasSelectedTag;
            await Dispatcher.BeginInvoke(() => Model.Preview.IsLoading = hasContent);
            if (!hasContent) return;

            try {
                if (Model.Preview.EnableRender) {
                    var builder = GetMaterialBuilder();
                    await builder.UpdateAllTexturesAsync(mergedToken);

                    await Dispatcher.BeginInvoke(() => {
                        mergedToken.ThrowIfCancellationRequested();

                        UpdateMaterial();
                        Model.Preview.IsLoading = false;
                    });
                }
                else {
                    var img = await GetLayerImageSourceAsync(Model.Preview.SelectedTag, mergedToken);

                    await Dispatcher.BeginInvoke(() => {
                        if (mergedToken.IsCancellationRequested) return;

                        Model.Preview.LayerImage = img;
                        Model.Preview.IsLoading = false;
                    });
                }
            }
            catch (OperationCanceledException) {}
        }

        public async Task UpdateLayerAsync(CancellationToken token = default)
        {
            await Dispatcher.BeginInvoke(() => Model.Preview.IsLoading = true);

            if (Model.Preview.EnableRender) {
                var builder = GetMaterialBuilder();
                await builder.UpdateTexturesByTagAsync(Model.Preview.SelectedTag, token);

                await Dispatcher.BeginInvoke(() => {
                    UpdateMaterial();
                    Model.Preview.IsLoading = false;
                });
            }
            else {
                var img = await GetLayerImageSourceAsync(Model.Preview.SelectedTag, token);

                await Dispatcher.BeginInvoke(() => {
                    Model.Preview.LayerImage = img;
                    Model.Preview.IsLoading = false;
                });
            }
        }

        public void UpdateMaterial()
        {
            var builder = GetMaterialBuilder();

            var enableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling
                ?? RenderPreviewSettings.Default_EnableLinearSampling;

            builder.ColorSampler = enableLinearSampling
                ? CustomSamplerStates.Color_Linear
                : CustomSamplerStates.Color_Point;

            builder.HeightSampler = enableLinearSampling
                ? CustomSamplerStates.Height_Linear
                : CustomSamplerStates.Height_Point;

            builder.PassName = Model.Preview.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
                _ => null,
            };

            builder.PassNameOIT = Model.Preview.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
                _ => null,
            };

            var mat = builder.BuildMaterial();
            if (mat.CanFreeze) mat.Freeze();

            Model.Preview.ModelMaterial = mat;
        }

        public void Cancel()
        {
            lock (lockHandle) {
                tokenSource?.Cancel();
            }
        }

        private IMaterialBuilder GetMaterialBuilder()
        {
            return Model.Preview.RenderMode switch {
                RenderPreviewModes.Diffuse => builderDiffuse,
                RenderPreviewModes.PbrFilament => builderPbr,
                RenderPreviewModes.PbrJessie => builderPbr,
                RenderPreviewModes.PbrNull => builderPbr,
                _ => throw new ApplicationException(),
            };
        }

        private void OnShaderCompileErrors(ShaderCompileError[] errors)
        {
            var e = new ShaderCompileErrorEventArgs {
                Errors = errors,
            };

            ShaderCompileErrors?.Invoke(this, e);
        }

        private async void OnEnableRenderChanged(object sender, EventArgs e)
        {
            await Task.Run(async () => {
                if (Model.Preview.EnableRender) {
                    await UpdateAsync(false);
                }
                else {
                    await UpdateLayerAsync();
                }
            });
        }

        private async void OnRenderModeChanged(object sender, EventArgs e)
        {
            if (!Model.Preview.EnableRender) return;

            await Task.Run(() => UpdateAsync(false));
        }

        private void OnRenderSceneChanged(object sender, EventArgs e)
        {
            if (Model.Preview.EnableRender)
                UpdateMaterial();
        }

        private async void OnSelectedTagChanged(object sender, EventArgs e)
        {
            if (!Model.Preview.EnableRender) {
                await Task.Run(() => UpdateLayerAsync());
            }
        }

        private async Task<ImageSource> GetLayerImageSourceAsync(string tag, CancellationToken token)
        {
            using var previewBuilder = provider.GetRequiredService<ILayerPreviewBuilder>();

            previewBuilder.Input = Model.PackInput;
            previewBuilder.Profile = Model.Profile.Loaded;
            previewBuilder.Material = Model.Material.Loaded;

            using var image = await previewBuilder.BuildAsync(tag, 0);
            return await CreateImageSourceAsync(image, token);
        }

        private static MeshGeometry3D BuildCube(int size)
        {
            var builder = new MeshBuilder(true, true, true);
            builder.AddCubeFace(Vector3.Zero, new Vector3(1, 0, 0), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(-1, 0, 0), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, 0, 1), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, 0, -1), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, 1, 0), Vector3.UnitX, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, -1, 0), Vector3.UnitX, size, size, size);
            builder.ComputeTangents(MeshFaces.Default);

            //builder.AddBox(Vector3.Zero, size, size, size);
            //builder.ComputeNormalsAndTangents(MeshFaces.Default);

            return builder.ToMeshGeometry3D();
        }

        private static async Task<ImageSource> CreateImageSourceAsync(Image image, CancellationToken token)
        {
            await using var stream = new MemoryStream();
            await image.SaveAsync(stream, BmpFormat.Instance, token);
            await stream.FlushAsync(token);
            stream.Seek(0, SeekOrigin.Begin);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.CacheOption = BitmapCacheOption.OnLoad;
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            imageSource.Freeze();

            return imageSource;
        }
    }

    internal class ShaderCompileErrorEventArgs : EventArgs
    {
        public ShaderCompileError[] Errors {get; set;}
    }
}
