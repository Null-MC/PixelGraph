using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlazedTerracottaBlue : BedrockBlockData
    {
        public const string BlockId = "glazed_terracotta_blue";
        public const string BlockName = "Glazed Terracotta Blue";


        public GlazedTerracottaBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueGlazedTerracotta.BlockId,
            });
        }
    }
}
