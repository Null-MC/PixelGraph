using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlassPaneTopBlack : BedrockBlockData
    {
        public const string BlockId = "glass_pane_top_black";
        public const string BlockName = "Glass Pane Top Black";


        public GlassPaneTopBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackStainedGlassPaneTop.BlockId,
            });
        }
    }
}
