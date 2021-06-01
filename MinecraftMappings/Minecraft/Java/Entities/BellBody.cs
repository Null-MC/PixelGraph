using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class BellBody : JavaEntityData
    {
        public const string EntityId = "bell_body";
        public const string EntityName = "Bell Body";


        public BellBody() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                Path = "bell",
            });
        }
    }
}
