using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlazedTerracottaBrown : BedrockBlockData
    {
        public const string BlockId = "glazed_terracotta_brown";
        public const string BlockName = "Glazed Terracotta Brown";


        public GlazedTerracottaBrown() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BrownGlazedTerracotta.BlockId;
            });
        }
    }
}
