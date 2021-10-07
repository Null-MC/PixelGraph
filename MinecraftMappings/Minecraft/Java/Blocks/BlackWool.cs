using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackWool : JavaBlockData
    {
        public const string BlockId = "black_wool";
        public const string BlockName = "Black Wool";


        public BlackWool() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.WoolColoredBlack.BlockId,
            });
        }
    }
}
