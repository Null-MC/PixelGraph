using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BellSide : BedrockBlockData
    {
        public const string BlockId = "bell_side";
        public const string BlockName = "Bell Side";


        public BellSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BellSide.BlockId,
            });
        }
    }
}
