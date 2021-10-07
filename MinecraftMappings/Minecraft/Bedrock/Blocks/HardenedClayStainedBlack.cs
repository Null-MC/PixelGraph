using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class HardenedClayStainedBlack : BedrockBlockData
    {
        public const string BlockId = "hardened_clay_stained_black";
        public const string BlockName = "Hardened Clay Stained Black";


        public HardenedClayStainedBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackTerracotta.BlockId,
            });
        }
    }
}
