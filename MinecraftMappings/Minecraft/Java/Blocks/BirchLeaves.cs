using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchLeaves : JavaBlockData
    {
        public const string BlockId = "birch_leaves";
        public const string BlockName = "Birch Leaves";


        public BirchLeaves() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.LeavesBirch.BlockId,
            });
        }
    }
}
