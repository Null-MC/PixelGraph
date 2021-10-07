using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BellBottom : JavaBlockData
    {
        public const string BlockId = "bell_bottom";
        public const string BlockName = "Bell Bottom";


        public BellBottom() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BellBottom.BlockId,
            });
        }
    }
}
