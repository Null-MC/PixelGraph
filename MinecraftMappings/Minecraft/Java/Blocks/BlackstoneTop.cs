using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackstoneTop : JavaBlockData
    {
        public const string BlockId = "blackstone_top";
        public const string BlockName = "Blackstone Top";


        public BlackstoneTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BlackstoneTop.BlockId,
            });
        }
    }
}
