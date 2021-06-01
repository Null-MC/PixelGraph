using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackTerracotta : JavaBlockData
    {
        public const string BlockId = "black_terracotta";
        public const string BlockName = "Black Terracotta";


        public BlackTerracotta() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.HardenedClayStainedBlack.BlockId,
            });
        }
    }
}
