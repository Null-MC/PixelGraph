using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeetrootsStage0 : BedrockBlockData
    {
        public const string BlockId = "beetroots_stage0";
        public const string BlockName = "Beetroots Stage 0";


        public BeetrootsStage0() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeetrootsStage0.BlockId,
            });
        }
    }
}
