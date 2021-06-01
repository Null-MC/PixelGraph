using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class HardenedClayStainedBlue : BedrockBlockData
    {
        public const string BlockId = "hardened_clay_stained_blue";
        public const string BlockName = "Hardened Clay Stained Blue";


        public HardenedClayStainedBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueTerracotta.BlockId,
            });
        }
    }
}
