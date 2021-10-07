using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class SaplingBirch : BedrockBlockData
    {
        public const string BlockId = "sapling_birch";
        public const string BlockName = "Sapling Birch";


        public SaplingBirch() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BirchSapling.BlockId,
            });
        }
    }
}
