using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Block;
using MinecraftMappings.Internal.Models.Entity;
using PixelGraph.Common;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Rendering;
using PixelGraph.Rendering.Models;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Helix.Controls;
using PixelGraph.UI.Helix.Materials;
using PixelGraph.UI.Internal.IO;
using PixelGraph.UI.Internal.IO.Models;
using PixelGraph.UI.Internal.Projects;
using SharpDX;
using SharpDX.Direct3D11;
using System.IO;

namespace PixelGraph.UI.Helix.Models;

public class MultiPartMeshBuilder(
    IServiceProvider provider,
    IProjectContextManager projectContextMgr,
    MinecraftResourceLocator locator,
    ModelLoader modelLoader) : IDisposable
{
    private const float CubeSize = 4f;

    private readonly Dictionary<string, IMaterialBuilder> materialMap = [];
    private readonly List<(IModelBuilder, IMaterialBuilder)> partsList = [];
    private bool isEntity;

    public ObservableElement3DCollection ModelParts { get; } = [];


    public void Dispose()
    {
        ClearTextureBuilders();
        GC.SuppressFinalize(this);
    }

    public async Task BuildAsync(IRenderContext renderContext, CancellationToken token = default)
    {
        ClearTextureBuilders();
        partsList.Clear();

        var entityModel = modelLoader.GetJavaEntityModel(renderContext.DefaultMaterial);
        if (entityModel != null) {
            await BuildEntityModelAsync(renderContext, entityModel, token);
            return;
        }

        // TODO: add an item model builder

        var blockModel = modelLoader.GetBlockModel(renderContext.DefaultMaterial, true);
        if (blockModel != null) {
            FlattenBlockModelTextures(blockModel);

            await BuildMaterialMapAsync(renderContext, blockModel.Textures, token);
            
            // Apply default material if no textures are mapped
            if (materialMap.Count == 0) {
                var materialBuilder = UpdateMaterial(renderContext, renderContext.DefaultMaterial);
                await materialBuilder.UpdateAllTexturesAsync(0, token);

                materialMap["all"] = materialBuilder;

                foreach (var element in blockModel.Elements)
                    element.SetTexture("#all");
            }

            BuildBlockModel(blockModel, renderContext.EnableTiling);
        }
    }

    /// <summary>
    /// Used to update the model bindings. Must be performed synchronously!
    /// </summary>
    public void UpdateModelParts()
    {
        ModelParts.Clear();

        foreach (var (modelBuilder, materialBuilder) in partsList) {
            var modelPart = new BlockMeshGeometryModel3D {
                Geometry = modelBuilder.ToBlockMeshGeometry3D(),
                Material = materialBuilder.BuildMaterial(),
                IsThrowingShadow = true,
                CullMode = isEntity
                    ? CullMode.None
                    : CullMode.Back,
            };

            // TODO: I don't think this works at all...
            if (modelPart.Material.CanFreeze) modelPart.Material.Freeze();

            ModelParts.Add(modelPart);
        }
    }

    private static void FlattenBlockModelTextures(BlockModelVersion model)
    {
        var remappedKeys = model.Textures
            .Where(p => p.Value != null && p.Value.StartsWith('#'))
            .Select(p => p.Key).ToArray();

        foreach (var textureId in remappedKeys) {
            var remappedFile = model.Textures[textureId];

            while (remappedFile?.StartsWith('#') ?? false) {
                var aliasName = remappedFile[1..];
                if (!remappedKeys.Contains(aliasName)) break;

                if (!model.Textures.TryGetValue(aliasName, out var aliasMatchName)) break;
                remappedFile = aliasMatchName;
            }

            // TODO: remap model parts
            foreach (var element in model.Elements) {
                foreach (var face in ModelElement.AllFaces) {
                    var faceData = element.GetFace(face);
                    if (faceData == null) continue;

                    if (!string.Equals(faceData.Texture, $"#{textureId}")) continue;

                    faceData.Texture = remappedFile;
                }
            }
        }

        foreach (var textureId in remappedKeys)
            model.Textures.Remove(textureId);
    }

    private void BuildBlockModel(BlockModelVersion model, bool tile = false)
    {
        isEntity = false;

        foreach (var (textureId, matBuilder) in materialMap) {
            var modelBuilder = new BlockModelBuilder();

            if (tile) {
                for (var z = -1; z <= 1; z++) {
                    for (var y = -1; y <= 1; y++) {
                        for (var x = -1; x <= 1; x++) {
                            Vector3 offset;
                            offset.X = x * 16f;
                            offset.Y = y * 16f;
                            offset.Z = z * 16f;

                            modelBuilder.AppendModelTextureParts(CubeSize, offset, model, textureId);
                        }
                    }
                }
            }
            else {
                modelBuilder.AppendModelTextureParts(CubeSize, Vector3.Zero, model, textureId);
            }

            partsList.Add((modelBuilder, matBuilder));
        }
    }

    private async Task BuildEntityModelAsync(IRenderContext renderContext, EntityModelVersion model, CancellationToken token)
    {
        isEntity = true;

        var modelBuilder = new EntityModelBuilder();
        modelBuilder.BuildEntity(CubeSize, model);

        var materialBuilder = UpdateMaterial(renderContext, renderContext.DefaultMaterial);
        await materialBuilder.UpdateAllTexturesAsync(0, token);

        partsList.Add((modelBuilder, materialBuilder));
    }

    public void ClearTextureBuilders()
    {
        foreach (var builder in materialMap.Values)
            builder.Dispose();

        materialMap.Clear();
    }

    //public void UpdateTabEnvironmentMaps(bool environmentEnabled)
    //{
    //    UpdateModelParts();

    //    foreach (var part in ModelParts.OfType<BlockMeshGeometryModel3D>()) {
    //        part.Material.
    //    }

    //    foreach (var mat in materialMap.Values) {
    //        //mat.RenderEnvironmentMap = environmentEnabled;
    //        ModelParts[0].Material.
    //        // TODO: ?
    //    }
    //}

    private async Task BuildMaterialMapAsync(IRenderContext renderContext, IReadOnlyDictionary<string, string> textureMap, CancellationToken token)
    {
        ClearTextureBuilders();

        var projectContext = projectContextMgr.GetContextRequired();
        var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

        serviceBuilder.Initialize();
        serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

        await using var scope = serviceBuilder.Build();

        var matReader = scope.GetRequiredService<IMaterialReader>();

        foreach (var (textureId, textureFile) in textureMap) {
            if (string.Equals(textureId, "particle", StringComparison.InvariantCultureIgnoreCase)) continue;

            // find material from textureFile
            MaterialProperties? material;
            var partIndex = 0;

            var fileName = Path.GetFileNameWithoutExtension(textureFile);
            if (string.Equals(fileName, renderContext.DefaultMaterial.Name, StringComparison.InvariantCultureIgnoreCase)) {
                material = renderContext.DefaultMaterial;
            }
            else if (renderContext.DefaultMaterial.TryGetPartIndex(fileName, out partIndex)) {
                material = renderContext.DefaultMaterial;
            }
            //else if (renderContext.DefaultMaterial.CTM.Method)
            else if (locator.FindLocalMaterial(textureFile, out var materialFile) && materialFile != null) {
                material = await matReader.LoadAsync(materialFile, token);
            }
            else {
                //throw new ApplicationException($"Unable to locate material '{textureFile}'!");
                // TODO: log error, missing texture

                //material = renderContext.MissingMaterial;
                material = renderContext.DefaultMaterial;
            }

            var materialBuilder = UpdateMaterial(renderContext, material);
            await materialBuilder.UpdateAllTexturesAsync(partIndex, token);

            materialMap[textureId] = materialBuilder;
        }
    }

    public void UpdateMaterials(IRenderContext renderContext)
    {
        foreach (var materialBuilder in materialMap.Values) {
            materialBuilder.HeightSampler = renderContext.EnableLinearSampling
                ? CustomSamplerStates.Height_Linear
                : CustomSamplerStates.Height_Point;

            materialBuilder.EnvironmentCubeMapSource = renderContext.EnvironmentCubeMap;
            materialBuilder.RenderEnvironmentMap = renderContext.EnvironmentEnabled;
            materialBuilder.DielectricBrdfLutMapSource = renderContext.DielectricBrdfLutMap;
            materialBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;
        }
    }

    public void InvalidateMaterials()
    {
        foreach (var builder in materialMap.Values)
            builder.ClearAllTextures();
    }

    private IMaterialBuilder CreateMaterialBuilder(RenderPreviewModes renderMode)
    {
        return renderMode switch {
            RenderPreviewModes.Diffuse => new DiffuseMaterialBuilder(provider),
            RenderPreviewModes.Normals => new NormalsMaterialBuilder(provider),
            RenderPreviewModes.PbrFilament => new OldPbrMaterialBuilder(provider),
            RenderPreviewModes.PbrJessie => new LabPbrMaterialBuilder(provider),
            RenderPreviewModes.PbrNull => new LabPbrMaterialBuilder(provider),
            _ => throw new ApplicationException($"Unknown render mode '{renderMode}'!")
        };
    }

    private IMaterialBuilder UpdateMaterial(IRenderContext renderContext, MaterialProperties material)
    {
        var materialBuilder = CreateMaterialBuilder(renderContext.RenderMode);

        materialBuilder.PackContext = new ProjectPublishContext {
            //RootPath = projectContext.RootDirectory,
            Project = renderContext.Project,
            Profile = renderContext.PackProfile,
        };

        materialBuilder.Material = material;
        materialBuilder.EnvironmentCubeMapSource = renderContext.EnvironmentCubeMap;
        materialBuilder.RenderEnvironmentMap = renderContext.EnvironmentEnabled;
        materialBuilder.DielectricBrdfLutMapSource = renderContext.DielectricBrdfLutMap;
        materialBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;

        //if (materialBuilder is LabPbrMaterialBuilder labPbrBuilder) {
        //    labPbrBuilder.DielectricBrdfLutMapSource = renderContext.DielectricBrdfLutMap;
        //    labPbrBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;
        //    //labPbrBuilder.BrdfLutMap = renderContext.BrdfLutMap;
        //}
        //else if (materialBuilder is OldPbrMaterialBuilder oldPbrBuilder) {
        //    oldPbrBuilder.DielectricBrdfLutMapSource = renderContext.DielectricBrdfLutMap;
        //    oldPbrBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;
        //    //oldPbrBuilder.BrdfLutMap = renderContext.BrdfLutMap;
        //}

        materialBuilder.ColorSampler = CustomSamplerStates.Color_Point;

        materialBuilder.HeightSampler = renderContext.EnableLinearSampling
            ? CustomSamplerStates.Height_Linear
            : CustomSamplerStates.Height_Point;

        materialBuilder.PassName = renderContext.RenderMode switch {
            RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
            RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
            RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
            _ => null,
        };

        //materialBuilder.PassNameOIT = renderContext.RenderMode switch {
        //    RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
        //    RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
        //    RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
        //    _ => null,
        //};

        return materialBuilder;
    }
}
