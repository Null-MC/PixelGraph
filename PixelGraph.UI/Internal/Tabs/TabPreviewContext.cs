using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

#if !NORENDER
using PixelGraph.Rendering.Models;
using PixelGraph.UI.Helix.Models;
#endif

namespace PixelGraph.UI.Internal.Tabs
{
    public class TabPreviewContext : IDisposable
    {
//#if !NORENDER
//        private static readonly Dictionary<string, Func<IModelBuilder, BlockMeshGeometry3D>> map = new(StringComparer.InvariantCultureIgnoreCase) {
//                [ModelType.Bell] = builder => builder.BuildEntity(CubeSize, new BellBody().GetLatestVersion()),
//                [ModelType.Boat] = builder => builder.BuildEntity(CubeSize, new Boat().GetLatestVersion()),
//                [ModelType.Cow] = builder => builder.BuildEntity(CubeSize, new Cow().GetLatestVersion()),
//                [ModelType.Cube] = builder => builder.BuildCube(CubeSize),
//                [ModelType.Plane] = builder => builder.BuildCube(CubeSize, 4, 1, 4),
//                [ModelType.Zombie] = builder => builder.BuildEntity(CubeSize, new Zombie().GetLatestVersion()),
//            };
//#endif

        private readonly IServiceProvider provider;
        //private readonly IAppSettings appSettings;
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
        public MultiPartMeshBuilder Mesh {get;}
        //private IMaterialBuilder materialBuilder;
        //private IModelBuilder modelBuilder;
        //public ObservableElement3DCollection ModelParts {get; private set;}

        //public Material ModelMaterial {get; set;}
#endif


        public TabPreviewContext(IServiceProvider provider)
        {
            this.provider = provider;

            lockHandle = new object();

#if !RELEASENORENDER
            Mesh = new MultiPartMeshBuilder(provider);
#endif
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#if !NORENDER
        public async Task BuildModelMeshAsync(RenderPreviewModes renderMode, IRenderContext renderContext, CancellationToken token = default)
        {
            var mergedToken = StartNewToken(token);
            //var builder = GetMaterialBuilder(mainModel, sceneModel, renderModel);

            //await Task.Run(() => builder.UpdateAllTexturesAsync(mergedToken), mergedToken);

            //await Task.Run(() => Mesh.Build(renderMode, renderContext), mergedToken);
            await Mesh.BuildAsync(renderMode, renderContext, mergedToken);
            //Mesh.UpdateModelParts();

            //IsMaterialBuilderValid = true;
        }

        public void UpdateModelParts()
        {
            //var mergedToken = StartNewToken(token);
            //var builder = GetMaterialBuilder(mainModel, sceneModel, renderModel);

            //await Task.Run(() => builder.UpdateAllTexturesAsync(mergedToken), mergedToken);

            //await Task.Run(() => Mesh.Build(renderMode, renderContext), mergedToken);
            Mesh.UpdateModelParts();
            IsMaterialBuilderValid = true;
        }
#endif

        public async Task BuildLayerAsync(ResourcePackInputProperties packInput, ResourcePackProfileProperties packProfile, MaterialProperties material, string textureTag, CancellationToken token = default)
        {
            //var mergedToken = StartNewToken(token);

            //var material = (model.SelectedTab as MaterialTabModel)?.Material;
            // WARN: (i think) There's a race condition between loading material and updating preview
            //if (material == null) return;

            using var previewBuilder = provider.GetRequiredService<ILayerPreviewBuilder>();

            previewBuilder.Input = packInput;
            previewBuilder.Profile = packProfile;
            previewBuilder.Material = material; 

            var tag = textureTag;
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
        //public Material UpdateMaterial(MainWindowModel mainModel, ScenePropertiesModel sceneModel, RenderPreviewModel renderModel)
        //{
        //    var builder = GetMaterialBuilder(mainModel, sceneModel, renderModel);

        //    var enableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling
        //        ?? RenderPreviewSettings.Default_EnableLinearSampling;

        //    //builder.ColorSampler = enableLinearSampling
        //    //    ? CustomSamplerStates.Color_Linear
        //    //    : CustomSamplerStates.Color_Point;
        //    builder.ColorSampler = CustomSamplerStates.Color_Point;

        //    builder.HeightSampler = enableLinearSampling
        //        ? CustomSamplerStates.Height_Linear
        //        : CustomSamplerStates.Height_Point;

        //    builder.PassName = renderModel.RenderMode switch {
        //        RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
        //        RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
        //        RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
        //        _ => null,
        //    };

        //    builder.PassNameOIT = renderModel.RenderMode switch {
        //        RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
        //        RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
        //        RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
        //        _ => null,
        //    };

        //    ModelMaterial = builder.BuildMaterial();
        //    if (ModelMaterial.CanFreeze) ModelMaterial.Freeze();
        //    IsMaterialValid = true;

        //    return ModelMaterial;
        //}

        //public ObservableElement3DCollection UpdateModel(MainWindowModel mainModel)
        //{
        //    var material = mainModel.SelectedTabMaterial;
        //    if (material == null) return null;

        //    try {
        //        var model = BuildModelFile(material);
        //        if (model != null) {
        //            // TODO: 
                    
                    
        //            return model;
        //        }
        //    }
        //    catch (Exception) {
        //        // TODO: log error!
        //    }

        //    var mesh = new MultiTexturedMesh();
        //    BlockMeshGeometry3D meshPart;

        //    if (material.ModelType != null) {
        //        if (map.TryGetValue(material.ModelType, out var meshFunc)) {
        //            meshPart = meshFunc(modelBuilder);
        //            mesh.Set("texture", material.LocalFilename, meshPart);
        //            return mesh;
        //        }
        //        else {
        //            //throw new ApplicationException($"Unknown model type '{Model.ModelType}'!");
        //            // TODO: log error!
        //        }
        //    }

        //    //mesh.Set("texture", material.LocalFilename, meshPart);
        //    //return mesh;

        //    var modelPart = new BlockMeshGeometryModel3D();
        //    modelPart.Geometry = modelBuilder.BuildCube(CubeSize);

        //    // TODO: load actual material textures
        //    part.Material = material;

        //    RenderModel.MeshParts.Add(part);
        //}

        public void InvalidateMaterialBuilder(bool clear)
        {
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
            //if (clear) ModelMaterial = null;
            if (clear) Mesh.ClearTextureBuilders();
        }

        public void InvalidateMaterial(bool clear)
        {
            IsMaterialValid = false;
            //if (clear) ModelMaterial = null;
            if (clear) Mesh.ClearTextureBuilders();
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
            //materialBuilder?.Dispose();
            Mesh.Dispose();
#endif

            LayerImage = null;
            tokenSource?.Dispose();
            IsLayerValid = false;
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
        }
        
#if !NORENDER

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
