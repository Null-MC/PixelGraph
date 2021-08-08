using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Items
{
    public class Apple : JavaItemData
    {
        public const string BlockId = "apple";
        public const string BlockName = "Apple";


        public Apple() : base(BlockName)
        {
            AddVersion(BlockId);
        }
    }
}
