using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaLeaves : JavaBlockData
    {
        public const string BlockId = "acacia_leaves";
        public const string BlockName = "Acacia Leaves";


        public AcaciaLeaves() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.LeavesAcacia.BlockId,
            });
        }
    }
}
