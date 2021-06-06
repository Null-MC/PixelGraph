using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview.Materials;
using PixelGraph.UI.Internal.Preview.Scene;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Shaders;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using SharpDX;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    internal class PreviewViewModel : IDisposable, IAsyncDisposable
    {
        private readonly IServiceProvider provider;
        private readonly Dispatcher uiDispatcher;
        private readonly object lockHandle;

        private DiffuseMaterialBuilder builderDiffuse;
        private PbrMetalMaterialBuilder builderPbrMetal;
        private PbrSpecularMaterialBuilder builderPbrSpecular;
        private Stream _skyImageStream;

        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public MainModel Model {get; set;}


        public PreviewViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            lockHandle = new object();
            uiDispatcher = Application.Current.Dispatcher;
        }

        public void Dispose()
        {
            builderDiffuse?.Dispose();
            builderPbrMetal?.Dispose();
            builderPbrSpecular?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (builderDiffuse != null)
                await builderDiffuse.DisposeAsync();

            if (builderPbrMetal != null)
                await builderPbrMetal.DisposeAsync();

            if (builderPbrSpecular != null)
                await builderPbrSpecular.DisposeAsync();
        }

        public void Initialize()
        {
            InitializeShaders();

            ReloadShaders();
            
            Model.Preview.Camera = new PerspectiveCamera();

            Model.Preview.SunCamera = new PerspectiveCamera { 
                UpDirection = new Media3D.Vector3D(1, 0, 0), 
                FarPlaneDistance = 5000, 
                NearPlaneDistance = 1,
                FieldOfView = 45
            };

            ResetViewport();

            _skyImageStream = ResourceLoader.Open("PixelGraph.UI.Resources.sky.dds");
            Model.Preview.SkyTexture = _skyImageStream;

            Model.Preview.Model = BuildCube(4);

            builderDiffuse = new DiffuseMaterialBuilder(provider) {Model = Model};
            builderPbrMetal = new PbrMetalMaterialBuilder(provider) {Model = Model};
            builderPbrSpecular = new PbrSpecularMaterialBuilder(provider) {Model = Model};

            Model.Preview.EnableRenderChanged += OnEnableRenderChanged;
            Model.Preview.RenderModeChanged += OnRenderModeChanged;
            Model.Preview.RenderSceneChanged += OnRenderSceneChanged;
            Model.Preview.SelectedTagChanged += OnSelectedTagChanged;
        }

        private void InitializeShaders()
        {
            var shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            shaderMgr.Add(CustomShaderNames.PbrSpecularVertex, new ShaderSourceDescription {
                Profile = "vs_4_0",
                RawFileName = "vs_pbr_specular.hlsl",
                CompiledResourceName = "vs_pbr_specular.cso",
            });

            shaderMgr.Add(CustomShaderNames.PbrSpecularPixel, new ShaderSourceDescription {
                Profile = "ps_4_0",
                RawFileName = "ps_pbr_specular.hlsl",
                CompiledResourceName = "ps_pbr_specular.cso",
            });
        }

        public void ReloadShaders()
        {
            var shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            if (!shaderMgr.LoadAll(out var compileErrors))
                OnShaderCompileErrors(compileErrors);

            Model.Preview.EffectsManager?.Dispose();
            Model.Preview.EffectsManager = new CustomEffectsManager(shaderMgr);
        }

        public void ResetViewport()
        {
            Model.Preview.Camera.Position = new Media3D.Point3D(10, 10, 10);
            Model.Preview.Camera.LookDirection = new Media3D.Vector3D(-10, -10, -10);
            Model.Preview.Camera.UpDirection = new Media3D.Vector3D(0, 1, 0);

            Model.Preview.SunCamera.Position = new Media3D.Point3D(6, 10, 0);
            Model.Preview.SunCamera.LookDirection = new Media3D.Vector3D(-1, -3, 0);
        }

        public async Task ClearAsync()
        {
            await uiDispatcher.BeginInvoke(() => Model.Preview.LayerImage = null);

            await builderDiffuse.ClearAllTexturesAsync();
            await builderPbrMetal.ClearAllTexturesAsync();
            await builderPbrSpecular.ClearAllTexturesAsync();
        }

        public async Task SetFromFileAsync(string filename)
        {
            await uiDispatcher.BeginInvoke(() => Model.Preview.IsLoading = true);

            var texImage = new BitmapImage();

            texImage.BeginInit();
            texImage.CacheOption = BitmapCacheOption.OnLoad;
            texImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            texImage.UriSource = new Uri(filename);
            texImage.EndInit();

            texImage.Freeze();

            await uiDispatcher.BeginInvoke(() => {
                Model.Preview.LayerImage = texImage;
                Model.Preview.IsLoading = false;
            });
        }

        public async Task UpdateAsync(bool clear, CancellationToken token = default)
        {
            if (clear) await ClearAsync();

            var hasContent = Model.Material.HasLoaded; // && VM.HasSelectedTag;
            await uiDispatcher.BeginInvoke(() => Model.Preview.IsLoading = hasContent);
            if (!hasContent) return;

            if (Model.Preview.EnableRender) {
                var builder = GetMaterialBuilder();
                await builder.UpdateAllTexturesAsync(token);

                await uiDispatcher.BeginInvoke(() => {
                    UpdateMaterial();
                    Model.Preview.IsLoading = false;
                });
            }
            else {
                var img = await GetLayerImageSourceAsync(Model.Preview.SelectedTag, token);

                await uiDispatcher.BeginInvoke(() => {
                    Model.Preview.LayerImage = img;
                    Model.Preview.IsLoading = false;
                });
            }
        }

        public async Task UpdateLayerAsync(CancellationToken token = default)
        {
            await uiDispatcher.BeginInvoke(() => Model.Preview.IsLoading = true);

            if (Model.Preview.EnableRender) {
                var builder = GetMaterialBuilder();
                await builder.UpdateTexturesByTagAsync(Model.Preview.SelectedTag, token);

                await uiDispatcher.BeginInvoke(() => {
                    UpdateMaterial();
                    Model.Preview.IsLoading = false;
                });
            }
            else {
                var img = await GetLayerImageSourceAsync(Model.Preview.SelectedTag, token);

                await uiDispatcher.BeginInvoke(() => {
                    Model.Preview.LayerImage = img;
                    Model.Preview.IsLoading = false;
                });
            }
        }

        public void UpdateMaterial()
        {
            var builder = GetMaterialBuilder();
            var mat = builder.BuildMaterial();
            mat.Freeze();

            Model.Preview.ModelMaterial = mat;
        }

        public void Cancel()
        {
            lock (lockHandle) {
                throw new NotImplementedException();
            }
        }

        private IMaterialBuilder GetMaterialBuilder()
        {
            return Model.Preview.RenderMode switch {
                RenderPreviewModes.Diffuse => builderDiffuse,
                RenderPreviewModes.PbrMetal => builderPbrMetal,
                RenderPreviewModes.PbrSpecular => builderPbrSpecular,
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
            builder.AddCubeFace(Vector3.Zero, new Vector3( 1,  0,  0), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(-1,  0,  0), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3( 0,  0,  1), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3( 0,  0, -1), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3( 0,  1,  0), Vector3.UnitZ, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3( 0, -1,  0), Vector3.UnitZ, size, size, size);
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
