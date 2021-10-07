using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BlastFurnaceFrontOff : BedrockBlockData
    {
        public const string BlockId = "blast_furnace_front_off";
        public const string BlockName = "Blast Furnace Front Off";


        public BlastFurnaceFrontOff() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlastFurnaceFront.BlockId,
            });
        }
    }
}
