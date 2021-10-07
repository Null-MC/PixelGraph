using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueTerracotta : JavaBlockData
    {
        public const string BlockId = "blue_terracotta";
        public const string BlockName = "Blue Terracotta";


        public BlueTerracotta() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.HardenedClayStainedBlue.BlockId,
            });
        }
    }
}
