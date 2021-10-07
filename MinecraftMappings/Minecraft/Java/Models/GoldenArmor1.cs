using MinecraftMappings.Internal.Entities;
using System.Linq;

namespace MinecraftMappings.Minecraft.Java.Models
{
    public class GoldenArmor1 : Armor1
    {
        public const string EntityId = "gold_layer_1";
        public const string EntityName = "Golden Armor [1]";


        public GoldenArmor1() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                TextVersion = "1.0.0",
                Path = "models/armor/gold_layer_1",
                UVMappings = DefaultUVRegions.ToList(),
            });
        }
    }
}
