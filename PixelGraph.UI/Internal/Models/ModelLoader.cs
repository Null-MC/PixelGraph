using MinecraftMappings.Internal.Models.Block;
using MinecraftMappings.Internal.Models.Entity;
using MinecraftMappings.Internal.Textures.Block;
using MinecraftMappings.Internal.Textures.Entity;
using MinecraftMappings.Minecraft;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using System;
using System.IO;
using System.Linq;

namespace PixelGraph.UI.Internal.Models
{
    public interface IModelLoader
    {
        JavaEntityModelVersion GetEntityModel(MaterialProperties material);
        BlockModelVersion GetBlockModel(MaterialProperties material);
    }

    internal class ModelLoader : IModelLoader
    {
        private readonly IBlockModelParser parser;


        public ModelLoader(IBlockModelParser parser)
        {
            this.parser = parser;
        }

        public JavaEntityModelVersion GetEntityModel(MaterialProperties material)
        {
            if (material.Model == null && IsEntityPath(material.LocalPath)) {
                var entityVersion = Minecraft.Java.GetEntityModelForTexture<JavaEntityTextureVersion>(material.Name, material.LocalPath)?.GetLatestVersion();

                if (entityVersion != null) return entityVersion;
            }

            if (material.Model?.StartsWith("entity/", StringComparison.InvariantCultureIgnoreCase) ?? false) {
                var modelId = Path.GetFileName(material.Model);
                var entityVersion = Minecraft.Java.FindEntityModelVersionById<JavaEntityModelVersion>(modelId).FirstOrDefault();

                if (entityVersion != null) return entityVersion;
            }

            return null;
        }

        public BlockModelVersion GetBlockModel(MaterialProperties material)
        {
            var modelFile = material.Model;

            if (modelFile == null && IsEntityPath(material.LocalPath)) {
                return null;
            }

            if (modelFile?.StartsWith("entity/", StringComparison.InvariantCultureIgnoreCase) ?? false) {
                return null;
            }

            if (modelFile == null) {
                var modelData = Minecraft.Java.GetBlockModelForTexture<JavaBlockTextureVersion>(material.Name);
                modelFile = modelData?.GetLatestVersion()?.Id;
            }

            if (modelFile == null) {
                var model = parser.LoadRecursive("blocks/cube_all");

                if (model != null) {
                    model.Textures["all"] = material.LocalFilename;
                    return model;
                }
            }

            return parser.LoadRecursive(modelFile);
        }

        private static bool IsEntityPath(string materialPath)
        {
            return PathEx.Normalize(materialPath).Contains("minecraft/textures/entity");

            // TODO
            return false;
        }
    }
}
