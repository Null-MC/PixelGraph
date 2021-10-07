using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueWool : JavaBlockData
    {
        public const string BlockId = "blue_wool";
        public const string BlockName = "Blue Wool";


        public BlueWool() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.WoolColoredBlue.BlockId,
            });
        }
    }
}
