using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeeNestFront : BedrockBlockData
    {
        public const string BlockId = "bee_nest_front";
        public const string BlockName = "Bee Nest Front";


        public BeeNestFront() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeeNestFront.BlockId,
            });
        }
    }
}
