using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BellSide : JavaBlockData
    {
        public const string BlockId = "bell_side";
        public const string BlockName = "Bell Side";


        public BellSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BellSide.BlockId,
            });
        }
    }
}
