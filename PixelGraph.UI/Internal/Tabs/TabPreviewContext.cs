using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview;
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
using System.Windows.Media.Imaging;
using MinecraftMappings.Internal.Blocks;
using MinecraftMappings.Minecraft;
using MinecraftMappings.Minecraft.Java.Models.Entity;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.Models;

#if !NORENDER
using System.IO;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.UI.Helix.Materials;
using PixelGraph.UI.Helix.Models;
using PixelGraph.UI.Helix.Shaders;
using PixelGraph.UI.Models.Scene;
#endif

namespace PixelGraph.UI.Internal.Tabs
{
    public class TabPreviewContext : IDisposable
    {
        private const float CubeSize = 4f;

        private static readonly Dictionary<string, Func<IModelBuilder, BlockMeshGeometry3D>> map = new(StringComparer.InvariantCultureIgnoreCase) {
                [ModelType.Bell] = builder => builder.BuildEntity(CubeSize, new BellBody().GetLatestVersion()),
                [ModelType.Boat] = builder => builder.BuildEntity(CubeSize, new Boat().GetLatestVersion()),
                [ModelType.Cow] = builder => builder.BuildEntity(CubeSize, new Cow().GetLatestVersion()),
                [ModelType.Cube] = builder => builder.BuildCube(CubeSize),
                [ModelType.Plane] = builder => builder.BuildCube(CubeSize, 4, 1, 4),
                [ModelType.Zombie] = builder => builder.BuildEntity(CubeSize, new Zombie().GetLatestVersion()),
            };

        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly object lockHandle;
        private CancellationTokenSource tokenSource;
        private BitmapSource _layerImageSource;

        public Guid Id {get; set;}
        public string SourceFile {get; set;}
        public Image<Rgb24> LayerImage {get; set;}
        public bool IsMaterialBuilderValid {get; private set;}
        public bool IsMaterialValid {get; private set;}
        public bool IsLayerValid {get; private set;}
        public bool IsLayerSourceValid {get; private set;}

        //public string CurrentLayerTag {get; private set;}

#if !NORENDER
        private IMaterialBuilder materialBuilder;
        private IModelBuilder modelBuilder;

        public Material ModelMaterial {get; set;}
#endif


        public TabPreviewContext(IServiceProvider provider)
        {
            this.provider = provider;

            appSettings = provider.GetRequiredService<IAppSettings>();
            modelBuilder = provider.GetRequiredService<IModelBuilder>();

            lockHandle = new object();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #if !NORENDER
        public async Task BuildMaterialAsync(MainWindowModel mainModel, ScenePropertiesModel sceneModel, RenderPreviewModel renderModel, CancellationToken token = default)
        {
            var mergedToken = StartNewToken(token);
            var builder = GetMaterialBuilder(mainModel, sceneModel, renderModel);

            await Task.Run(() => builder.UpdateAllTexturesAsync(mergedToken), mergedToken);

            IsMaterialBuilderValid = true;
        }
#endif

        public async Task BuildLayerAsync(MainWindowModel model, CancellationToken token = default)
        {
            //var mergedToken = StartNewToken(token);

            var material = (model.SelectedTab as MaterialTabModel)?.Material;
            // WARN: (i think) There's a race condition between loading material and updating preview
            if (material == null) return;

            using var previewBuilder = provider.GetRequiredService<ILayerPreviewBuilder>();

            previewBuilder.Input = model.PackInput;
            previewBuilder.Profile = model.Profile.Loaded;
            previewBuilder.Material = material;

            var tag = model.SelectedTag;
            if (TextureTags.Is(tag, TextureTags.General))
                tag = TextureTags.Color;

            LayerImage = await previewBuilder.BuildAsync<Rgb24>(tag, 0);
            //CurrentLayerTag = model.Preview.SelectedTag;
            IsLayerValid = true;
        }

        public BitmapSource GetLayerImageSource()
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

        #if !NORENDER
        public Material UpdateMaterial(MainWindowModel mainModel, ScenePropertiesModel sceneModel, RenderPreviewModel renderModel)
        {
            var builder = GetMaterialBuilder(mainModel, sceneModel, renderModel);

            var enableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling
                ?? RenderPreviewSettings.Default_EnableLinearSampling;

            //builder.ColorSampler = enableLinearSampling
            //    ? CustomSamplerStates.Color_Linear
            //    : CustomSamplerStates.Color_Point;
            builder.ColorSampler = CustomSamplerStates.Color_Point;

            builder.HeightSampler = enableLinearSampling
                ? CustomSamplerStates.Height_Linear
                : CustomSamplerStates.Height_Point;

            builder.PassName = renderModel.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
                _ => null,
            };

            builder.PassNameOIT = renderModel.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
                _ => null,
            };

            ModelMaterial = builder.BuildMaterial();
            if (ModelMaterial.CanFreeze) ModelMaterial.Freeze();
            IsMaterialValid = true;

            return ModelMaterial;
        }

        public BlockMeshGeometry3D UpdateModel(MainWindowModel mainModel)
        {
            var material = mainModel.SelectedTabMaterial;
            if (material == null) return null;

            try {
                var model = BuildModelFile(material);
                if (model != null) return model;
            }
            catch (Exception) {
                // TODO: log error!
            }

            if (material.ModelType != null) {
                if (map.TryGetValue(material.ModelType, out var meshFunc)) {
                    return meshFunc(modelBuilder);
                }
                else {
                    //throw new ApplicationException($"Unknown model type '{Model.ModelType}'!");
                    // TODO: log error!
                }
            }

            return modelBuilder.BuildCube(CubeSize);
        }

        public void InvalidateMaterialBuilder(bool clear)
        {
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
            if (clear) ModelMaterial = null;
        }

        public void InvalidateMaterial(bool clear)
        {
            IsMaterialValid = false;
            if (clear) ModelMaterial = null;
        }
#endif

        //public void Invalidate(bool clear)
        //{
        //    InvalidateMaterial(clear);
        //    InvalidateLayer(clear);
        //}

        public void InvalidateLayer(bool clear)
        {
            IsLayerValid = false;
            IsLayerSourceValid = false;

            if (clear) {
                LayerImage = null;
                _layerImageSource = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
#if !NORENDER
            materialBuilder?.Dispose();
#endif

            LayerImage = null;
            tokenSource?.Dispose();
            IsLayerValid = false;
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
        }
        
        #if !NORENDER
        private IMaterialBuilder GetMaterialBuilder(MainWindowModel mainModel, ScenePropertiesModel sceneModel, RenderPreviewModel renderModel)
        {
            switch (renderModel.RenderMode) {
                case RenderPreviewModes.Diffuse:
                    if (materialBuilder is DiffuseMaterialBuilder diffuseBuilder) {
                        diffuseBuilder.PackInput = mainModel.PackInput;
                        diffuseBuilder.PackProfile = mainModel.Profile.Loaded;
                        diffuseBuilder.Material = (mainModel.SelectedTab as MaterialTabModel)?.Material;
                        diffuseBuilder.EnvironmentCubeMapSource = renderModel.EnvironmentCube;
                        diffuseBuilder.RenderEnvironmentMap = sceneModel.SunEnabled;
                        return diffuseBuilder;
                    }

                    materialBuilder?.Dispose();

                    materialBuilder = new DiffuseMaterialBuilder(provider) {
                        PackInput = mainModel.PackInput,
                        PackProfile = mainModel.Profile.Loaded,
                        Material = (mainModel.SelectedTab as MaterialTabModel)?.Material,
                        EnvironmentCubeMapSource = renderModel.EnvironmentCube,
                        RenderEnvironmentMap = sceneModel.SunEnabled,
                    };

                    return materialBuilder;

                case RenderPreviewModes.PbrFilament:
                case RenderPreviewModes.PbrJessie:
                case RenderPreviewModes.PbrNull:
                    if (materialBuilder is PbrMaterialBuilder pbrBuilder) {
                        pbrBuilder.PackInput = mainModel.PackInput;
                        pbrBuilder.PackProfile = mainModel.Profile.Loaded;
                        pbrBuilder.Material = (mainModel.SelectedTab as MaterialTabModel)?.Material;
                        pbrBuilder.EnvironmentCubeMapSource = renderModel.EnvironmentCube;
                        pbrBuilder.IrradianceCubeMapSource = renderModel.IrradianceCube;
                        pbrBuilder.RenderEnvironmentMap = sceneModel.SunEnabled;
                        return pbrBuilder;
                    }

                    materialBuilder?.Dispose();

                    materialBuilder = new PbrMaterialBuilder(provider) {
                        PackInput = mainModel.PackInput,
                        PackProfile = mainModel.Profile.Loaded,
                        Material = (mainModel.SelectedTab as MaterialTabModel)?.Material,
                        EnvironmentCubeMapSource = renderModel.EnvironmentCube,
                        IrradianceCubeMapSource = renderModel.IrradianceCube,
                        RenderEnvironmentMap = sceneModel.SunEnabled,
                        BrdfLutMap = renderModel.BrdfLutMap,
                    };

                    return materialBuilder;

                default:
                    throw new ApplicationException();
            }
        }

        private BlockMeshGeometry3D BuildModelFile(MaterialProperties material)
        {
            var modelFile = material.ModelFile;

            if (modelFile == null) {
                var modelData = Minecraft.Java.GetModelForTexture<JavaBlockDataVersion>(material.Name);
                modelFile = modelData?.GetLatestVersion()?.Id;
            }

            if (modelFile == null) return null;

            var reader = provider.GetRequiredService<IInputReader>();
            var parser = provider.GetRequiredService<IBlockModelParser>();

            var filename = reader.GetFullPath(modelFile);
            var localPath = Path.GetDirectoryName(filename);
            var localFile = Path.GetFileName(filename);

            var model = parser.LoadRecursive(localPath, localFile);
            if (model == null) throw new ApplicationException($"Failed to load model file '{modelFile}'!");

            return modelBuilder.BuildModel(CubeSize, model);
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
#endif

        private static BitmapSource BuildFileSource(string filename)
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
