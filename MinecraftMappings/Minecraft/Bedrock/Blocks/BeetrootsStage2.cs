using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeetrootsStage2 : BedrockBlockData
    {
        public const string BlockId = "beetroots_stage2";
        public const string BlockName = "Beetroots Stage 2";


        public BeetrootsStage2() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeetrootsStage2.BlockId,
            });
        }
    }
}
