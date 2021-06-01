using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueGlazedTerracotta : JavaBlockData
    {
        public const string BlockId = "blue_glazed_terracotta";
        public const string BlockName = "Blue Glazed Terracotta";


        public BlueGlazedTerracotta() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.GlazedTerracottaBlue.BlockId,
            });
        }
    }
}
