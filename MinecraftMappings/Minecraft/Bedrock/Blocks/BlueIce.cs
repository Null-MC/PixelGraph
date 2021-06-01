using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BlueIce : BedrockBlockData
    {
        public const string BlockId = "blue_ice";
        public const string BlockName = "Blue Ice";


        public BlueIce() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueIce.BlockId,
            });
        }
    }
}
