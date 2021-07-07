using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class Alex : JavaEntityData
    {
        public const string EntityId = "alex";
        public const string EntityName = "Alex";


        public Alex() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
            });
        }
    }
}
