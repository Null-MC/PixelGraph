using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BellBottom : BedrockBlockData
    {
        public const string BlockId = "bell_bottom";
        public const string BlockName = "Bell Bottom";


        public BellBottom() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BellBottom.BlockId,
            });
        }
    }
}
