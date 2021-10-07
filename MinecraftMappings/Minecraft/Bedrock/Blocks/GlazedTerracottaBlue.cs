using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlazedTerracottaBlue : BedrockBlockData
    {
        public const string BlockId = "glazed_terracotta_blue";
        public const string BlockName = "Glazed Terracotta Blue";


        public GlazedTerracottaBlue() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BlueGlazedTerracotta.BlockId;
            });
        }
    }
}
