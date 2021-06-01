using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Bookshelf : JavaBlockData
    {
        public const string BlockId = "bookshelf";
        public const string BlockName = "Bookshelf";


        public Bookshelf() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.Bookshelf.BlockId,
            });
        }
    }
}
