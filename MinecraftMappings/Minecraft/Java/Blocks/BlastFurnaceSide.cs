using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlastFurnaceSide : JavaBlockData
    {
        public const string BlockId = "blast_furnace_side";
        public const string BlockName = "Blast Furnace Side";


        public BlastFurnaceSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BlastFurnaceSide.BlockId,
            });
        }
    }
}
