using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeehiveSide : BedrockBlockData
    {
        public const string BlockId = "beehive_side";
        public const string BlockName = "Beehive Side";


        public BeehiveSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeehiveSide.BlockId,
            });
        }
    }
}
