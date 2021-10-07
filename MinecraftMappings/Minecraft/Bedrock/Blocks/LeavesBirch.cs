using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class LeavesBirch : BedrockBlockData
    {
        public const string BlockId = "leaves_birch";
        public const string BlockName = "Leaves Birch";


        public LeavesBirch() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BirchLeaves.BlockId,
            });
        }
    }
}
