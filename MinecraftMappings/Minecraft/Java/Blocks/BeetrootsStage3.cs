using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeetrootsStage3 : JavaBlockData
    {
        public const string BlockId = "beetroots_stage3";
        public const string BlockName = "Beetroots Stage 3";


        public BeetrootsStage3() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeetrootsStage3.BlockId,
            });
        }
    }
}
