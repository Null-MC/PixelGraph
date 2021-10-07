using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueOrchid : JavaBlockData
    {
        public const string BlockId = "blue_orchid";
        public const string BlockName = "Blue Orchid";


        public BlueOrchid() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.FlowerBlueOrchid.BlockId,
            });
        }
    }
}
