using MinecraftMappings.Internal.Entities;
using System.Linq;

namespace MinecraftMappings.Minecraft.Java.Models
{
    public class IronArmor1 : Armor1
    {
        public const string EntityId = "iron_layer_1";
        public const string EntityName = "Iron Armor [1]";


        public IronArmor1() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                TextVersion = "1.0.0",
                Path = "models/armor/iron_layer_1",
                UVMappings = DefaultUVRegions.ToList(),
            });
        }
    }
}
