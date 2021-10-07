using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BellTop : JavaBlockData
    {
        public const string BlockId = "bell_top";
        public const string BlockName = "Bell Top";


        public BellTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BellTop.BlockId,
            });
        }
    }
}
