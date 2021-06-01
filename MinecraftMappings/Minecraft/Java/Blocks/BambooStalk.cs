using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BambooStalk : JavaBlockData
    {
        public const string BlockId = "bamboo_stalk";
        public const string BlockName = "Bamboo Stalk";


        public BambooStalk() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BambooStem.BlockId,
            });
        }
    }
}
