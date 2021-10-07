using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeetrootsStage0 : JavaBlockData
    {
        public const string BlockId = "beetroots_stage0";
        public const string BlockName = "Beetroots Stage 0";


        public BeetrootsStage0() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeetrootsStage0.BlockId,
            });
        }
    }
}
