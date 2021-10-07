using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlastFurnaceTop : JavaBlockData
    {
        public const string BlockId = "blast_furnace_top";
        public const string BlockName = "Blast Furnace Top";


        public BlastFurnaceTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BlastFurnaceTop.BlockId,
            });
        }
    }
}
