using MinecraftMappings.Minecraft;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewData;

namespace PixelGraph.UI.ViewModels
{
    internal class NewMaterialViewModel
    {
        public NewMaterialModel Model {get; set;}


        public NewMaterialViewModel()
        {
            UpdateBlockList();
            UpdateLocation();
        }

        public void UpdateBlockList()
        {
            Model.GameObjectNames.Clear();

            switch (Model.GameObjectType) {
                case GameObjectTypes.Block:
                case GameObjectTypes.OptifineCtm:
                    foreach (var block in Minecraft.Java.AllBlocks) {
                        var latest = block.GetLatestVersion();

                        Model.GameObjectNames.Add(new GameObjectOption {
                            Id = latest.Id,
                        });
                    }
                    break;
                case GameObjectTypes.Entity:
                    foreach (var entity in Minecraft.Java.AllEntities) {
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
            var path = GetPathForType();
            if (path == null) return null;

            if (Model.GameObjectName == null) return path;
            return $"{path}/{Model.GameObjectName}";
        }

        private string GetPathForType()
        {
            return Model.GameObjectType switch {
                GameObjectTypes.Block => "assets/minecraft/textures/block",
                GameObjectTypes.Entity => "assets/minecraft/textures/entity",
                GameObjectTypes.OptifineCtm => "assets/minecraft/optifine/ctm",
                _ => null,
            };
        }
    }
}
