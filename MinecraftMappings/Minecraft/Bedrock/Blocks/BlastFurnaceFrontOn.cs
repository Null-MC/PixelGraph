using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BlastFurnaceFrontOn : BedrockBlockData
    {
        public const string BlockId = "blast_furnace_front_on";
        public const string BlockName = "Blast Furnace Front On";


        public BlastFurnaceFrontOn() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlastFurnaceFrontOn.BlockId,
                FrameCount = 2,
            });
        }
    }
}
