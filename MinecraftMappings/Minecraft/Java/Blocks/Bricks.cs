using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Bricks : JavaBlockData
    {
        public const string BlockId = "bricks";
        public const string BlockName = "Bricks";


        public Bricks() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.Brick.BlockId,
            });
        }
    }
}
