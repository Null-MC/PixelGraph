using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeehiveFront : JavaBlockData
    {
        public const string BlockId = "beehive_front";
        public const string BlockName = "Beehive Front";


        public BeehiveFront() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeehiveFront.BlockId,
            });
        }
    }
}
