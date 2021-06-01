using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeehiveSide : JavaBlockData
    {
        public const string BlockId = "beehive_side";
        public const string BlockName = "Beehive Side";


        public BeehiveSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeehiveSide.BlockId,
            });
        }
    }
}
