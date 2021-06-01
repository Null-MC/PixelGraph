using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeetrootsStage1 : BedrockBlockData
    {
        public const string BlockId = "beetroots_stage1";
        public const string BlockName = "Beetroots Stage 1";


        public BeetrootsStage1() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeetrootsStage1.BlockId,
            });
        }
    }
}
