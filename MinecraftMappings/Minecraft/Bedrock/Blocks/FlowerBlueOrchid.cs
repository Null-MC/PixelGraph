using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class FlowerBlueOrchid : BedrockBlockData
    {
        public const string BlockId = "flower_blue_orchid";
        public const string BlockName = "Flower Blue Orchid";


        public FlowerBlueOrchid() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueOrchid.BlockId,
            });
        }
    }
}
