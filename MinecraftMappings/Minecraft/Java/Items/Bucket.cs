using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Items
{
    public class Bucket : JavaItemData
    {
        public const string BlockId = "bucket";
        public const string BlockName = "Bucket";


        public Bucket() : base(BlockName)
        {
            AddVersion(BlockId);
        }
    }
}
