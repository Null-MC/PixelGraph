using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class Beacon : BedrockBlockData
    {
        public const string BlockId = "beacon";
        public const string BlockName = "Beacon";


        public Beacon() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Beacon.BlockId,
            });
        }
    }
}
