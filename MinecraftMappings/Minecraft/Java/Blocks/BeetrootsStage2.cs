using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeetrootsStage2 : JavaBlockData
    {
        public const string BlockId = "beetroots_stage2";
        public const string BlockName = "Beetroots Stage 2";


        public BeetrootsStage2() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeetrootsStage2.BlockId,
            });
        }
    }
}
