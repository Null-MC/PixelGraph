using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.Materials;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Tabs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Internal
{
    public interface ITabPreviewManager : IDisposable
    {
        IEnumerable<TabPreviewContext> All {get;}

        void Add(TabPreviewContext context);
        TabPreviewContext Get(Guid id);
        void Remove(Guid id);
        void Clear();
        void InvalidateAll();
        void InvalidateAllMaterialBuilders(bool clear);
        void InvalidateAllMaterials(bool clear);
    }

    public class TabPreviewManager : ITabPreviewManager
    {
        private readonly Dictionary<Guid, TabPreviewContext> contextMap;

        public IEnumerable<TabPreviewContext> All => contextMap.Values;


        public TabPreviewManager()
        {
            contextMap = new Dictionary<Guid, TabPreviewContext>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Add(TabPreviewContext context)
        {
            contextMap[context.Id] = context;
        }

        public TabPreviewContext Get(Guid id)
        {
            return contextMap.TryGetValue(id, out var result) ? result : null;
        }

        public void Remove(Guid id)
        {
            contextMap.Remove(id);
        }

        public void Clear()
        {
            foreach (var context in All) context.Dispose();
            contextMap.Clear();
        }

        public void InvalidateAll()
        {
            foreach (var context in All)
                context.Invalidate();
        }

        public void InvalidateAllMaterialBuilders(bool clear)
        {
            foreach (var context in All) {
                context.IsMaterialBuilderValid = false;
                context.IsMaterialValid = false;

                if (clear) {
                    context.ModelMaterial = null;
                }
            }
        }

        public void InvalidateAllMaterials(bool clear)
        {
            foreach (var context in All) {
                context.IsMaterialValid = false;

                if (clear) {
                    context.ModelMaterial = null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            Clear();
        }
    }

    public class TabPreviewContext : IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly object lockHandle;
        private CancellationTokenSource tokenSource;
        private IMaterialBuilder materialBuilder;
        private ImageSource _layerImageSource;

        public Guid Id {get; set;}
        public string SourceFile {get; set;}
        public Material ModelMaterial {get; set;}
        public Image<Rgb24> LayerImage {get; set;}
        public bool IsMaterialBuilderValid {get; set;}
        public bool IsMaterialValid {get; set;}
        public bool IsLayerValid {get; set;}
        public bool IsLayerSourceValid {get; set;}

        public string CurrentLayerTag {get; private set;}


        public TabPreviewContext(IServiceProvider provider)
        {
            this.provider = provider;

            appSettings = provider.GetRequiredService<IAppSettings>();

            lockHandle = new object();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task BuildMaterialAsync(MainWindowModel model, CancellationToken token = default)
        {
            var mergedToken = StartNewToken(token);

            var builder = GetMaterialBuilder(model);
            await builder.UpdateAllTexturesAsync(mergedToken);

            IsMaterialBuilderValid = true;
        }

        public async Task BuildLayerAsync(MainWindowModel model, CancellationToken token = default)
        {
            //var mergedToken = StartNewToken(token);

            using var previewBuilder = provider.GetRequiredService<ILayerPreviewBuilder>();

            previewBuilder.Input = model.PackInput;
            previewBuilder.Profile = model.Profile.Loaded;
            previewBuilder.Material = (model.SelectedTab as MaterialTabModel)?.Material;

            LayerImage = await previewBuilder.BuildAsync<Rgb24>(model.Preview.SelectedTag, 0);
            CurrentLayerTag = model.Preview.SelectedTag;
            IsLayerValid = true;
        }

        public ImageSource GetLayerImageSource()
        {
            if (IsLayerSourceValid) return _layerImageSource;

            if (SourceFile != null) {
                var texImage = BuildFileSource(SourceFile);

                if (texImage.CanFreeze) texImage.Freeze();
                IsLayerSourceValid = true;
                return _layerImageSource = texImage;
            }

            if (!IsLayerValid) return null;

            var texImage2 = new ImageSharpSource<Rgb24>(LayerImage);
            if (texImage2.CanFreeze) texImage2.Freeze();

            IsLayerSourceValid = true;
            return _layerImageSource = texImage2;
        }

        public void UpdateMaterial(MainWindowModel model)
        {
            var builder = GetMaterialBuilder(model);

            var enableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling
                ?? RenderPreviewSettings.Default_EnableLinearSampling;

            //builder.ColorSampler = enableLinearSampling
            //    ? CustomSamplerStates.Color_Linear
            //    : CustomSamplerStates.Color_Point;
            builder.ColorSampler = CustomSamplerStates.Color_Point;

            builder.HeightSampler = enableLinearSampling
                ? CustomSamplerStates.Height_Linear
                : CustomSamplerStates.Height_Point;

            builder.PassName = model.Preview.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
                _ => null,
            };

            builder.PassNameOIT = model.Preview.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
                _ => null,
            };

            ModelMaterial = builder.BuildMaterial();
            if (ModelMaterial.CanFreeze) ModelMaterial.Freeze();
            IsMaterialValid = true;
        }

        public void Invalidate()
        {
            IsLayerValid = false;
            IsLayerSourceValid = false;
            LayerImage = null;
            _layerImageSource = null;

            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
            ModelMaterial = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            LayerImage = null;
            materialBuilder?.Dispose();
            tokenSource?.Dispose();
            IsLayerValid = false;
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
        }
        
        private IMaterialBuilder GetMaterialBuilder(MainWindowModel model)
        {
            switch (model.Preview.RenderMode) {
                case RenderPreviewModes.Diffuse:
                    if (materialBuilder is DiffuseMaterialBuilder diffuseBuilder) {
                        diffuseBuilder.PackInput = model.PackInput;
                        diffuseBuilder.PackProfile = model.Profile.Loaded;
                        diffuseBuilder.Material = (model.SelectedTab as MaterialTabModel)?.Material;
                        diffuseBuilder.EnvironmentCubeMapSource = model.Preview.EnvironmentCube;
                        diffuseBuilder.RenderEnvironmentMap = model.Preview.EnableEnvironment;
                        return diffuseBuilder;
                    }

                    materialBuilder?.Dispose();

                    materialBuilder = new DiffuseMaterialBuilder(provider) {
                        PackInput = model.PackInput,
                        PackProfile = model.Profile.Loaded,
                        Material = (model.SelectedTab as MaterialTabModel)?.Material,
                        EnvironmentCubeMapSource = model.Preview.EnvironmentCube,
                        RenderEnvironmentMap = model.Preview.EnableEnvironment,
                    };

                    return materialBuilder;

                case RenderPreviewModes.PbrFilament:
                case RenderPreviewModes.PbrJessie:
                case RenderPreviewModes.PbrNull:
                    if (materialBuilder is PbrMaterialBuilder pbrBuilder) {
                        pbrBuilder.PackInput = model.PackInput;
                        pbrBuilder.PackProfile = model.Profile.Loaded;
                        pbrBuilder.Material = (model.SelectedTab as MaterialTabModel)?.Material;
                        pbrBuilder.EnvironmentCubeMapSource = model.Preview.EnvironmentCube;
                        pbrBuilder.IrradianceCubeMapSource = model.Preview.IrradianceCube;
                        pbrBuilder.RenderEnvironmentMap = model.Preview.EnableEnvironment;
                        return pbrBuilder;
                    }

                    materialBuilder?.Dispose();

                    materialBuilder = new PbrMaterialBuilder(provider) {
                        PackInput = model.PackInput,
                        PackProfile = model.Profile.Loaded,
                        Material = (model.SelectedTab as MaterialTabModel)?.Material,
                        EnvironmentCubeMapSource = model.Preview.EnvironmentCube,
                        IrradianceCubeMapSource = model.Preview.IrradianceCube,
                        RenderEnvironmentMap = model.Preview.EnableEnvironment,
                    };

                    return materialBuilder;

                default:
                    throw new ApplicationException();
            }
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

        private static ImageSource BuildFileSource(string filename)
        {
            var texImage = new BitmapImage();

            texImage.BeginInit();
            texImage.CacheOption = BitmapCacheOption.OnLoad;
            texImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            texImage.UriSource = new Uri(filename);
            texImage.EndInit();

            return texImage;
        }
    }
}
