using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BlastFurnaceSide : BedrockBlockData
    {
        public const string BlockId = "blast_furnace_side";
        public const string BlockName = "Blast Furnace Side";


        public BlastFurnaceSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlastFurnaceSide.BlockId,
            });
        }
    }
}
