using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Models;
using SharpDX;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using System;
using System.IO;
using System.Reflection;
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

        private Stream _skyImageStream;
        private Stream _albedoImageStream;
        private Stream _heightImageStream;
        private Stream _normalImageStream;
        private Stream _occlusionRoughMetalImageStream;
        private Stream _emissiveImageStream;

        public MainModel Model {get; set;}


        public PreviewViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            lockHandle = new object();
            uiDispatcher = Application.Current.Dispatcher;
        }

        public void Dispose()
        {
            _albedoImageStream?.Dispose();
            _heightImageStream?.Dispose();
            _normalImageStream?.Dispose();
            _occlusionRoughMetalImageStream?.Dispose();
            _emissiveImageStream?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await ClearAlbedoAsync();
            await ClearHeightAsync();
            await ClearNormalAsync();
            await ClearOcclusionRoughMetalAsync();
            await ClearEmissiveAsync();
        }

        public async Task InitializeAsync()
        {
            Model.Preview.EffectsManager = new DefaultEffectsManager();

            Model.Preview.Camera = new PerspectiveCamera();

            Model.Preview.SunCamera = new PerspectiveCamera { 
                UpDirection = new Media3D.Vector3D(1, 0, 0), 
                FarPlaneDistance = 5000, 
                NearPlaneDistance = 1,
                FieldOfView = 45
            };

            ResetViewport();

            _skyImageStream = await LoadFileToMemoryAsync("PixelGraph.UI.Resources.sky.dds");
            Model.Preview.SkyTexture = _skyImageStream;

            Model.Preview.Model = BuildCube(4);

            //Model.Preview.RenderLoadingText = new BillboardText3D {
            //    TextInfo = {
            //        new TextInfo("Updating Material...", Vector3.UnitZ * 4) {
            //            Background = Color4.Black,
            //            Foreground = Color.White,
            //            Scale = 2,
            //        },
            //    },
            //};

            Model.Preview.EnableRenderChanged += OnEnableRenderChanged;
            Model.Preview.RenderSceneChanged += OnRenderSceneChanged;
            Model.Preview.SelectedTagChanged += OnSelectedTagChanged;
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

            await ClearAlbedoAsync();
            await ClearHeightAsync();
            await ClearNormalAsync();
            await ClearOcclusionRoughMetalAsync();
            await ClearEmissiveAsync();
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
                await UpdateAlbedoAsync(token);
                await UpdateHeightAsync(token);
                await UpdateNormalAsync(token);
                await UpdateOcclusionRoughMetalAsync(token);
                await UpdateEmissiveAsync(token);

                await uiDispatcher.BeginInvoke(() => {
                    UpdateMaterial();
                    Model.Preview.IsLoading = false;
                });
            }
            else {
                using var previewBuilder = GetPreviewBuilder<ILayerPreviewBuilder>();
                var img = await GetLayerImageSourceAsync(previewBuilder, Model.Preview.SelectedTag, token);

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
                if (TextureTags.Is(Model.Preview.SelectedTag, TextureTags.Albedo))
                    await UpdateAlbedoAsync(token);
                else if (TextureTags.Is(Model.Preview.SelectedTag, TextureTags.Height))
                    await UpdateHeightAsync(token);
                else if (TextureTags.Is(Model.Preview.SelectedTag, TextureTags.Normal))
                    await UpdateNormalAsync(token);
                else if (TextureTags.Is(Model.Preview.SelectedTag, TextureTags.Rough))
                    await UpdateOcclusionRoughMetalAsync(token);
                else if (TextureTags.Is(Model.Preview.SelectedTag, TextureTags.Emissive))
                    await UpdateEmissiveAsync(token);

                await uiDispatcher.BeginInvoke(() => {
                    UpdateMaterial();
                    Model.Preview.IsLoading = false;
                });
            }
            else {
                using var previewBuilder = GetPreviewBuilder<ILayerPreviewBuilder>();
                var img = await GetLayerImageSourceAsync(previewBuilder, Model.Preview.SelectedTag, token);

                await uiDispatcher.BeginInvoke(() => {
                    Model.Preview.LayerImage = img;
                    Model.Preview.IsLoading = false;
                });
            }
        }

        public void UpdateMaterial()
        {
            Model.Preview.ModelMaterial = BuildMaterial();
        }

        public void Cancel()
        {
            lock (lockHandle) {
                throw new NotImplementedException();
            }
        }

        private async Task UpdateAlbedoAsync(CancellationToken token = default)
        {
            await ClearAlbedoAsync();
            using var previewBuilder = GetPreviewBuilder<IRenderPreviewBuilder>();
            _albedoImageStream = await GetTextureStreamAsync(previewBuilder, TextureTags.Albedo, 0, 0, token);
        }

        private async Task UpdateHeightAsync(CancellationToken token = default)
        {
            await ClearHeightAsync();
            using var previewBuilder = GetPreviewBuilder<IRenderPreviewBuilder>();
            _heightImageStream = await GetTextureStreamAsync(previewBuilder, TextureTags.Height, 0, 0, token);
        }

        private async Task UpdateNormalAsync(CancellationToken token = default)
        {
            await ClearNormalAsync();
            using var previewBuilder = GetPreviewBuilder<IRenderPreviewBuilder>();
            _normalImageStream = await GetTextureStreamAsync(previewBuilder, TextureTags.Normal, 0, 0, token);
        }

        private async Task UpdateOcclusionRoughMetalAsync(CancellationToken token = default)
        {
            await ClearOcclusionRoughMetalAsync();
            using var previewBuilder = GetPreviewBuilder<IRenderPreviewBuilder>();
            _occlusionRoughMetalImageStream = await GetTextureStreamAsync(previewBuilder, TextureTags.Rough, 0, 0, token);
        }

        private async Task UpdateEmissiveAsync(CancellationToken token = default)
        {
            await ClearEmissiveAsync();
            using var previewBuilder = GetPreviewBuilder<IRenderPreviewBuilder>();
            _emissiveImageStream = await GetTextureStreamAsync(previewBuilder, TextureTags.Emissive, 0, 0, token);
        }

        private PBRMaterial BuildMaterial()
        {
            var mat = new PBRMaterial {
                EnableTessellation = false,
                EnableAutoTangent = true,
                RenderDisplacementMap = true,
                RenderEnvironmentMap = Model.Preview.EnableEnvironment,
                RenderShadowMap = true,

                MetallicFactor = 1.0,
                RoughnessFactor = 1.0,
                ReflectanceFactor = 0.8,
                //AmbientOcclusionFactor = 0.8,
                EmissiveColor = new Color4(1f, 1f, 1f, 0f),

                //MinTessellationDistance = 1,
                //MinDistanceTessellationFactor = 128,
                //MaxTessellationDistance = 48,
                //MaxDistanceTessellationFactor = 8,
                //DisplacementMapScaleMask = new Vector4(0.1f, 0.1f, 0.1f, 0f),
                SurfaceMapSampler = new SamplerStateDescription {
                    Filter = Filter.MinMagPointMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = Comparison.Never,
                    MaximumLod = int.MaxValue,
                    MinimumLod = 0,
                    MaximumAnisotropy = 16,
                },
            };

            if (_albedoImageStream != null)
                mat.AlbedoMap = TextureModel.Create(_albedoImageStream);

            if (_heightImageStream != null)
                mat.DisplacementMap = TextureModel.Create(_heightImageStream);

            if (_normalImageStream != null)
                mat.NormalMap = TextureModel.Create(_normalImageStream);

            if (_occlusionRoughMetalImageStream != null)
                mat.AmbientOcculsionMap = mat.RoughnessMetallicMap = TextureModel.Create(_occlusionRoughMetalImageStream);

            if (_emissiveImageStream != null)
                mat.EmissiveMap = TextureModel.Create(_emissiveImageStream);

            return mat;
        }

        private async Task ClearAlbedoAsync()
        {
            if (_albedoImageStream == null) return;
            await _albedoImageStream.DisposeAsync();
            _albedoImageStream = null;
        }

        private async Task ClearHeightAsync()
        {
            if (_heightImageStream == null) return;
            await _heightImageStream.DisposeAsync();
            _heightImageStream = null;
        }

        private async Task ClearNormalAsync()
        {
            if (_normalImageStream == null) return;
            await _normalImageStream.DisposeAsync();
            _normalImageStream = null;
        }

        private async Task ClearOcclusionRoughMetalAsync()
        {
            if (_occlusionRoughMetalImageStream == null) return;
            await _occlusionRoughMetalImageStream.DisposeAsync();
            _occlusionRoughMetalImageStream = null;
        }

        private async Task ClearEmissiveAsync()
        {
            if (_emissiveImageStream == null) return;
            await _emissiveImageStream.DisposeAsync();
            _emissiveImageStream = null;
        }

        private T GetPreviewBuilder<T>()
            where T : ITexturePreviewBuilder
        {
            var previewBuilder = provider.GetRequiredService<T>();

            previewBuilder.Input = Model.PackInput;
            previewBuilder.Profile = Model.Profile.Loaded;
            previewBuilder.Material = Model.Material.Loaded;
            
            return previewBuilder;
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

        private static async Task<ImageSource> GetLayerImageSourceAsync(ITexturePreviewBuilder previewBuilder, string tag, CancellationToken token)
        {
            using var image = await previewBuilder.BuildAsync(tag, 0);
            return await CreateImageSourceAsync(image, token);
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

        private static async Task<Stream> GetTextureStreamAsync(ITexturePreviewBuilder previewBuilder, string textureTag, int? frame = null, int? part = null, CancellationToken token = default)
        {
            var stream = new MemoryStream();

            try {
                using var image = await previewBuilder.BuildAsync(textureTag, frame, part);
                await image.SaveAsBmpAsync(stream, token);
                await stream.FlushAsync(token);
                stream.Position = 0;
                return stream;
            }
            catch {
                await stream.DisposeAsync();
                throw;
            }
        }

        public static async Task<MemoryStream> LoadFileToMemoryAsync(string filePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            await using var stream = assembly.GetManifestResourceStream(filePath);
            if (stream == null) throw new ApplicationException($"Unable to locate resource '{filePath}'!");

            //await using var file = new FileStream(filePath, FileMode.Open);

            var buffer = new MemoryStream();

            try {
                await stream.CopyToAsync(buffer);
                return buffer;
            }
            catch {
                await buffer.DisposeAsync();
                throw;
            }
        }
    }
}
