using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class LeavesAcacia : BedrockBlockData
    {
        public const string BlockId = "leaves_acacia";
        public const string BlockName = "Leaves Acacia";


        public LeavesAcacia() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AcaciaLeaves.BlockId,
            });
        }
    }
}
