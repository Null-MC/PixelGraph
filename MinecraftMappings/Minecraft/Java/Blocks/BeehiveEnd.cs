using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeehiveEnd : JavaBlockData
    {
        public const string BlockId = "beehive_end";
        public const string BlockName = "Beehive End";


        public BeehiveEnd() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeehiveTop.BlockId,
            });
        }
    }
}
