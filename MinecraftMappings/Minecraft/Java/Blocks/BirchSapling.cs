using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchSapling : JavaBlockData
    {
        public const string BlockId = "birch_sapling";
        public const string BlockName = "Birch Sapling";


        public BirchSapling() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.SaplingBirch.BlockId,
            });
        }
    }
}
