using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackGlazedTerracotta : JavaBlockData
    {
        public const string BlockId = "black_glazed_terracotta";
        public const string BlockName = "Black Glazed Terracotta";


        public BlackGlazedTerracotta() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.GlazedTerracottaBlack.BlockId,
            });
        }
    }
}
