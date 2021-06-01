using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeetrootsStage3 : BedrockBlockData
    {
        public const string BlockId = "beetroots_stage3";
        public const string BlockName = "Beetroots Stage 3";


        public BeetrootsStage3() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeetrootsStage3.BlockId,
            });
        }
    }
}
