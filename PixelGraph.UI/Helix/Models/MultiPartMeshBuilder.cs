using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Block;
using MinecraftMappings.Internal.Models.Entity;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Rendering;
using PixelGraph.Rendering.Models;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Helix.Controls;
using PixelGraph.UI.Helix.Materials;
using PixelGraph.UI.Internal.Models;
using PixelGraph.UI.Internal.Settings;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Helix.Models
{
    public class MultiPartMeshBuilder : IDisposable
    {
        private const float CubeSize = 4f;

        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly IModelLoader modelLoader;
        private readonly IMaterialReader materialReader;
        private readonly IMinecraftResourceLocator locator;
        private readonly Dictionary<string, IMaterialBuilder> materialMap;
        private readonly List<(IModelBuilder, IMaterialBuilder)> partsList;
        private bool isEntity;

        public ObservableElement3DCollection ModelParts {get;}


        public MultiPartMeshBuilder(IServiceProvider provider)
        {
            this.provider = provider;

            appSettings = provider.GetRequiredService<IAppSettings>();
            modelLoader = provider.GetRequiredService<IModelLoader>();
            materialReader = provider.GetRequiredService<IMaterialReader>();
            locator = provider.GetRequiredService<IMinecraftResourceLocator>();

            materialMap = new Dictionary<string, IMaterialBuilder>();
            ModelParts = new ObservableElement3DCollection();
            partsList = new List<(IModelBuilder, IMaterialBuilder)>();
        }

        public void Dispose()
        {
            ClearTextureBuilders();
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
                    IsThrowingShadow = true,
                    CullMode = isEntity
                        ? CullMode.None
                        : CullMode.Back,
                };

                modelPart.Geometry = modelBuilder.ToBlockMeshGeometry3D();
                modelPart.Material = materialBuilder.BuildMaterial();

                // TODO: I don't think this works at all...
                if (modelPart.Material.CanFreeze) modelPart.Material.Freeze();

                ModelParts.Add(modelPart);
            }
        }

        private void FlattenBlockModelTextures(BlockModelVersion model)
        {
            var remappedKeys = model.Textures
                .Where(p => p.Value.StartsWith('#'))
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
                    Vector3 offset;
                    for (var z = -1; z <= 1; z++) {
                        for (var y = -1; y <= 1; y++) {
                            for (var x = -1; x <= 1; x++) {
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

        private async Task BuildMaterialMapAsync(IRenderContext renderContext, IReadOnlyDictionary<string, string> textureMap, CancellationToken token)
        {
            ClearTextureBuilders();

            foreach (var (textureId, textureFile) in textureMap) {
                if (string.Equals(textureId, "particle", StringComparison.InvariantCultureIgnoreCase)) continue;

                // find material from textureFile
                MaterialProperties material;
                int partIndex = 0;

                var fileName = Path.GetFileNameWithoutExtension(textureFile);
                if (string.Equals(fileName, renderContext.DefaultMaterial.Name, StringComparison.InvariantCultureIgnoreCase)) {
                    material = renderContext.DefaultMaterial;
                }
                else if (renderContext.DefaultMaterial.TryGetPartIndex(fileName, out partIndex)) {
                    material = renderContext.DefaultMaterial;
                }
                //else if (renderContext.DefaultMaterial.CTM.Method)
                else if (locator.FindLocalMaterial(textureFile, out var materialFile)) {
                    material = await materialReader.LoadAsync(materialFile, token);
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

        public void InvalidateMaterials()
        {
            foreach (var builder in materialMap.Values)
                builder.ClearAllTextures();
        }

        private IMaterialBuilder CreateMaterialBuilder(RenderPreviewModes renderMode)
        {
            switch (renderMode) {
                case RenderPreviewModes.Diffuse:
                    return new DiffuseMaterialBuilder(provider);

                case RenderPreviewModes.Normals:
                    return new NormalsMaterialBuilder(provider);

                case RenderPreviewModes.PbrFilament:
                    return new OldPbrMaterialBuilder(provider);

                case RenderPreviewModes.PbrJessie:
                case RenderPreviewModes.PbrNull:
                    return new LabPbrMaterialBuilder(provider);

                default:
                    throw new ApplicationException($"Unknown render mode '{renderMode}'!");
            }
        }

        private IMaterialBuilder UpdateMaterial(IRenderContext renderContext, MaterialProperties material)
        {
            var materialBuilder = CreateMaterialBuilder(renderContext.RenderMode);

            materialBuilder.PackInput = renderContext.PackInput;
            materialBuilder.PackProfile = renderContext.PackProfile;
            materialBuilder.Material = material;

            materialBuilder.EnvironmentCubeMapSource = renderContext.EnvironmentCubeMap;
            materialBuilder.RenderEnvironmentMap = renderContext.EnvironmentEnabled;

            if (materialBuilder is LabPbrMaterialBuilder labPbrBuilder) {
                labPbrBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;
                labPbrBuilder.BrdfLutMap = renderContext.BrdfLutMap;
            }
            else if (materialBuilder is OldPbrMaterialBuilder oldPbrBuilder) {
                oldPbrBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;
                oldPbrBuilder.BrdfLutMap = renderContext.BrdfLutMap;
            }

            var enableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling
                                       ?? RenderPreviewSettings.Default_EnableLinearSampling;

            materialBuilder.ColorSampler = CustomSamplerStates.Color_Point;

            materialBuilder.HeightSampler = enableLinearSampling
                ? CustomSamplerStates.Height_Linear
                : CustomSamplerStates.Height_Point;

            materialBuilder.PassName = renderContext.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
                _ => null,
            };

            materialBuilder.PassNameOIT = renderContext.RenderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
                _ => null,
            };

            return materialBuilder;
        }
    }
}
