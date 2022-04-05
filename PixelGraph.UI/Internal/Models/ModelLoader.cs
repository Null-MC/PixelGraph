using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Internal.Models.Block;
using MinecraftMappings.Internal.Models.Entity;
using MinecraftMappings.Internal.Textures.Block;
using MinecraftMappings.Internal.Textures.Entity;
using MinecraftMappings.Minecraft;
using PixelGraph.Common;
using PixelGraph.Common.Material;
using System;
using System.IO;
using System.Linq;

namespace PixelGraph.UI.Internal.Models
{
    public class ModelLoader
    {
        private readonly IServiceProvider provider;
        private readonly IProjectContext projectContext;


        public ModelLoader(
            IServiceProvider provider,
            IProjectContext projectContext)
        {
            this.provider = provider;
            this.projectContext = projectContext;
        }

        public JavaEntityModelVersion GetJavaEntityModel(MaterialProperties material)
        {
            var modelFile = material.Model;
            if (modelFile == null && !MCPath.IsEntityPath(material.LocalPath)) return null;
            if (modelFile?.StartsWith("block/", StringComparison.InvariantCultureIgnoreCase) ?? false) return null;

            JavaEntityModelVersion baseModel;
            if (modelFile != null) {
                var modelId = Path.GetFileName(modelFile);
                baseModel = Minecraft.Java.FindEntityModelVersionById<JavaEntityModelVersion>(modelId)?.FirstOrDefault();
            }
            else {
                baseModel = Minecraft.Java.GetEntityModelForTexture<JavaEntityTextureVersion>(material.Name, material.LocalPath)?.GetLatestVersion();
                if (baseModel != null) modelFile = baseModel.Id;
            }

            if (baseModel == null) return null;

            if (modelFile != null) {
                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();
                serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
                serviceBuilder.Services.AddTransient<EntityModelParser>();

                using var scope = serviceBuilder.Build();

                var entityParser = scope.GetRequiredService<EntityModelParser>();
                entityParser.Build(baseModel, modelFile);
            }

            return baseModel;
        }

        public BlockModelVersion GetBlockModel(MaterialProperties material, bool defaultCube = false)
        {
            var modelFile = material.Model;
            if (modelFile == null && !MCPath.IsBlockPath(material.LocalPath) && !defaultCube) return null;

            if (modelFile == null) {
                var modelData = Minecraft.Java.GetBlockModelForTexture<JavaBlockTextureVersion>(material.Name);
                modelFile = modelData?.GetLatestVersion()?.Id;
            }

            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.Services.AddTransient<BlockModelParser>();

            using var scope = serviceBuilder.Build();

            var blockParser = scope.GetRequiredService<BlockModelParser>();

            if (modelFile == null && defaultCube) {
                var model = blockParser.LoadRecursive("block/cube_all");

                if (model != null) {
                    model.Textures["all"] = material.LocalFilename;
                    return model;
                }
            }

            return blockParser.LoadRecursive(modelFile);
        }
    }
}
