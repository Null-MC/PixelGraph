using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BlastFurnaceTop : BedrockBlockData
    {
        public const string BlockId = "blast_furnace_top";
        public const string BlockName = "Blast Furnace Top";


        public BlastFurnaceTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlastFurnaceTop.BlockId,
            });
        }
    }
}
