using MinecraftMappings.Minecraft;
using PixelGraph.UI.Models;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class ImportEntityFiltersViewModel
    {
        public ImportEntityFiltersModel Model {get; set;}


        public void UpdateEntityList()
        {
            Model.GameEntityList.Clear();

            if (string.IsNullOrWhiteSpace(Model.GameVersion)) return;
            var version = Version.Parse(Model.GameVersion);

            foreach (var entity in Minecraft.Java.AllEntities) {
                var entityVersion = entity.GetVersion(version);
                if (entityVersion == null) continue;

                var e = new GameEntityNameOption {
                    Name = entity.Name,
                    Data = entityVersion,
                };

                Model.GameEntityList.Add(e);
            }
        }
    }
}
