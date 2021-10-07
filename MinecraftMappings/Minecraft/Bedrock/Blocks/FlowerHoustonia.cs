using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class FlowerHoustonia : BedrockBlockData
    {
        public const string BlockId = "flower_houstonia";
        public const string BlockName = "Flower Houstonia";


        public FlowerHoustonia() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AzureBluet.BlockId,
            });
        }
    }
}
