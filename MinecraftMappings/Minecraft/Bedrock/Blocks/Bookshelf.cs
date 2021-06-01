using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class Bookshelf : BedrockBlockData
    {
        public const string BlockId = "bookshelf";
        public const string BlockName = "Bookshelf";


        public Bookshelf() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Bookshelf.BlockId,
            });
        }
    }
}
