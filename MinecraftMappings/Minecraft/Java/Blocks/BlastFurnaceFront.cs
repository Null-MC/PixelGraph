using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlastFurnaceFront : JavaBlockData
    {
        public const string BlockId = "blast_furnace_front";
        public const string BlockName = "Blast Furnace Front";


        public BlastFurnaceFront() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BlastFurnaceFrontOff.BlockId,
            });
        }
    }
}
