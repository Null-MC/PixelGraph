using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeehiveFrontHoney : JavaBlockData
    {
        public const string BlockId = "beehive_front_honey";
        public const string BlockName = "Beehive Front Honey";


        public BeehiveFrontHoney() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeehiveFrontHoney.BlockId,
            });
        }
    }
}
