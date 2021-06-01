using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeetrootsStage1 : JavaBlockData
    {
        public const string BlockId = "beetroots_stage1";
        public const string BlockName = "Beetroots Stage 1";


        public BeetrootsStage1() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeetrootsStage1.BlockId,
            });
        }
    }
}
