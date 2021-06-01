using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class FlowerAllium : BedrockBlockData
    {
        public const string BlockId = "flower_allium";
        public const string BlockName = "Flower Allium";


        public FlowerAllium() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Allium.BlockId,
            });
        }
    }
}
