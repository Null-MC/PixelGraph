using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Block;
using MinecraftMappings.Internal.Models.Entity;
using MinecraftMappings.Internal.Textures.Block;
using MinecraftMappings.Internal.Textures.Entity;
using MinecraftMappings.Minecraft;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Rendering.Models;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Helix.Controls;
using PixelGraph.UI.Helix.Materials;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Settings;
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
        private readonly IBlockModelParser parser;
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
            parser = provider.GetRequiredService<IBlockModelParser>();
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

        private bool IsEntityPath(string materialPath)
        {
            return PathEx.Normalize(materialPath).Contains("minecraft/textures/entity");

            // TODO
            return false;
        }

        public async Task BuildAsync(RenderPreviewModes renderMode, IRenderContext renderContext, CancellationToken token = default)
        {
            var modelFile = renderContext.DefaultMaterial.Model;
            isEntity = false;

            if (modelFile == null && IsEntityPath(renderContext.DefaultMaterial.LocalPath)) {
                var entityVersion = Minecraft.Java.GetEntityModelForTexture<JavaEntityTextureVersion>(renderContext.DefaultMaterial.Name)?.GetLatestVersion();

                if (entityVersion != null) {
                    await BuildEntityModelAsync(renderMode, renderContext, entityVersion, token);
                    return;
                }
            }

            if (modelFile?.StartsWith("entity/", StringComparison.InvariantCultureIgnoreCase) ?? false) {
                // Build Entity-Model
                var modelId = Path.GetFileName(modelFile);
                var entityVersion = Minecraft.Java.FindEntityModelVersionById<JavaEntityModelVersion>(modelId).FirstOrDefault();

                if (entityVersion != null) {
                    await BuildEntityModelAsync(renderMode, renderContext, entityVersion, token);
                    return;
                }
            }

            if (modelFile == null) {
                var modelData = Minecraft.Java.GetBlockModelForTexture<JavaBlockTextureVersion>(renderContext.DefaultMaterial.Name);
                modelFile = modelData?.GetLatestVersion()?.Id;
            }

            if (modelFile == null) {
                var model = parser.LoadRecursive("blocks/cube_all");
                model.Textures["all"] = renderContext.DefaultMaterial.LocalFilename;
                FlattenBlockModelTextures(model);

                ClearTextureBuilders();

                var materialBuilder = UpdateMaterial(renderMode, renderContext, renderContext.DefaultMaterial);
                await materialBuilder.UpdateAllTexturesAsync(0, token);

                materialMap["all"] = materialBuilder;

                BuildBlockModel(model);
                return;
            }

            // Build Block-Model
            var blockModel = parser.LoadRecursive(modelFile);
            if (blockModel == null) throw new ApplicationException($"Failed to load model file '{modelFile}'!");

            FlattenBlockModelTextures(blockModel);
            await BuildMaterialMapAsync(renderMode, renderContext, blockModel.Textures, token);
            
            // Apply default material if no textures are mapped
            if (materialMap.Count == 0) {
                var materialBuilder = UpdateMaterial(renderMode, renderContext, renderContext.DefaultMaterial);
                await materialBuilder.UpdateAllTexturesAsync(0, token);

                materialMap["all"] = materialBuilder;

                foreach (var element in blockModel.Elements) {
                    foreach (var face in ModelElement.AllFaces) {
                        var modelFace = element.GetFace(face);
                        if (modelFace != null)
                            modelFace.Texture = "#all";
                    }
                }
            }

            BuildBlockModel(blockModel);
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

        private void BuildBlockModel(BlockModelVersion model)
        {
            partsList.Clear();

            foreach (var (textureId, matBuilder) in materialMap) {
                var modelBuilder = new BlockModelBuilder();
                modelBuilder.BuildModel(CubeSize, model, textureId);
                partsList.Add((modelBuilder, matBuilder));
            }
        }

        private async Task BuildEntityModelAsync(RenderPreviewModes renderMode, IRenderContext renderContext, EntityModelVersion model, CancellationToken token)
        {
            ClearTextureBuilders();

            var modelBuilder = new EntityModelBuilder();
            modelBuilder.BuildEntity(CubeSize, model);

            var materialBuilder = UpdateMaterial(renderMode, renderContext, renderContext.DefaultMaterial);
            await materialBuilder.UpdateAllTexturesAsync(0, token);
            // using default/selected material instead of lookup

            partsList.Clear();
            partsList.Add((modelBuilder, materialBuilder));
            isEntity = true;
        }

        public void ClearTextureBuilders()
        {
            foreach (var builder in materialMap.Values)
                builder.Dispose();

            materialMap.Clear();
        }

        private async Task BuildMaterialMapAsync(RenderPreviewModes renderMode, IRenderContext renderContext, IReadOnlyDictionary<string, string> textureMap, CancellationToken token)
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
                else if (locator.FindLocalMaterial(textureFile, out var materialFile)) {
                    material = await materialReader.LoadAsync(materialFile, token);
                }
                else {
                    //throw new ApplicationException($"Unable to locate material '{textureFile}'!");
                    // TODO: log error, missing texture

                    material = renderContext.MissingMaterial;
                }

                var materialBuilder = UpdateMaterial(renderMode, renderContext, material);
                await materialBuilder.UpdateAllTexturesAsync(partIndex, token);

                materialMap[textureId] = materialBuilder;
            }
        }

        public void InvalidateMaterials()
        {
            foreach (var builder in materialMap.Values)
                builder.ClearAllTextures();
        }

        private IMaterialBuilder CreateMaterialBuilder(RenderPreviewModes renderMode, IRenderContext renderContext)
        {
            switch (renderMode) {
                case RenderPreviewModes.Diffuse:
                    return new DiffuseMaterialBuilder(provider);

                case RenderPreviewModes.PbrFilament:
                case RenderPreviewModes.PbrJessie:
                case RenderPreviewModes.PbrNull:
                    return new PbrMaterialBuilder(provider);

                default:
                    throw new ApplicationException($"Unknown render mode '{renderMode}'!");
            }
        }

        private IMaterialBuilder UpdateMaterial(RenderPreviewModes renderMode, IRenderContext renderContext, MaterialProperties material)
        {
            var materialBuilder = CreateMaterialBuilder(renderMode, renderContext);

            materialBuilder.PackInput = renderContext.PackInput;
            materialBuilder.PackProfile = renderContext.PackProfile;
            materialBuilder.Material = material;

            materialBuilder.EnvironmentCubeMapSource = renderContext.EnvironmentCubeMap;
            materialBuilder.RenderEnvironmentMap = renderContext.EnvironmentEnabled;

            if (materialBuilder is PbrMaterialBuilder pbrBuilder) {
                pbrBuilder.IrradianceCubeMapSource = renderContext.IrradianceCubeMap;
                pbrBuilder.BrdfLutMap = renderContext.BrdfLutMap;
            }

            var enableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling
                                       ?? RenderPreviewSettings.Default_EnableLinearSampling;

            materialBuilder.ColorSampler = CustomSamplerStates.Color_Point;

            materialBuilder.HeightSampler = enableLinearSampling
                ? CustomSamplerStates.Height_Linear
                : CustomSamplerStates.Height_Point;

            materialBuilder.PassName = renderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilament,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessie,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNull,
                _ => null,
            };

            materialBuilder.PassNameOIT = renderMode switch {
                RenderPreviewModes.PbrFilament => CustomPassNames.PbrFilamentOIT,
                RenderPreviewModes.PbrJessie => CustomPassNames.PbrJessieOIT,
                RenderPreviewModes.PbrNull => CustomPassNames.PbrNullOIT,
                _ => null,
            };

            return materialBuilder;
        }
    }
}
