using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueGlazedTerracotta : JavaBlockData
    {
        public const string BlockId = "blue_glazed_terracotta";
        public const string BlockName = "Blue Glazed Terracotta";


        public BlueGlazedTerracotta() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.GlazedTerracottaBlue.BlockId;
            });
        }
    }
}
