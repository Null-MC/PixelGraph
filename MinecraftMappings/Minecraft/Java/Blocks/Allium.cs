using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Allium : JavaBlockData
    {
        public const string BlockId = "allium";
        public const string BlockName = "Allium";


        public Allium() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.FlowerAllium.BlockId,
            });
        }
    }
}
