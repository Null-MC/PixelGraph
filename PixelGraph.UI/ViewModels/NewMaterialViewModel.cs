using MinecraftMappings.Minecraft;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewData;

namespace PixelGraph.UI.ViewModels
{
    internal class NewMaterialViewModel
    {
        public NewMaterialModel Model {get; set;}


        public void UpdateBlockList()
        {
            Model.GameObjectNames.Clear();

            switch (Model.GameObjectType) {
                case GameObjectTypes.Block:
                case GameObjectTypes.Optifine_CTM:
                    foreach (var block in Minecraft.Java.AllBlockTextures) {
                        var latest = block.GetLatestVersion();

                        Model.GameObjectNames.Add(new GameObjectOption {
                            Id = latest.Id,
                        });
                    }
                    break;
                case GameObjectTypes.Item:
                case GameObjectTypes.Optifine_CIT:
                    foreach (var item in Minecraft.Java.AllItems) {
                        var latest = item.GetLatestVersion();

                        Model.GameObjectNames.Add(new GameObjectOption {
                            Id = latest.Id,
                        });
                    }
                    break;
                case GameObjectTypes.Entity:
                    foreach (var entity in Minecraft.Java.AllEntityTextures) {
                        var latest = entity.GetLatestVersion();

                        var e = new GameObjectOption {
                            Id = latest.Id,
                        };

                        if (latest.Path != null)
                            e.Path = $"{latest.Path}/{latest.Id}";

                        Model.GameObjectNames.Add(e);
                    }
                    break;
            }
        }

        public void UpdateLocation()
        {
            Model.Location = GetLocation();
        }

        private string GetLocation()
        {
            var pathExt = GetPathForType(Model.GameObjectType);
            if (pathExt == null) return null;
            
            var path = $"assets/{Model.GameNamespace}/{pathExt}";
            if (Model.GameObjectName == null) return path;

            return $"{path}/{Model.GameObjectName}";
        }

        private static string GetPathForType(GameObjectTypes objType)
        {
            return objType switch {
                GameObjectTypes.Block => "textures/block",
                GameObjectTypes.Item => "textures/item",
                GameObjectTypes.Entity => "textures",
                GameObjectTypes.Optifine_CTM => "optifine/ctm",
                GameObjectTypes.Optifine_CIT => "optifine/cit",
                _ => null,
            };
        }
    }
}
