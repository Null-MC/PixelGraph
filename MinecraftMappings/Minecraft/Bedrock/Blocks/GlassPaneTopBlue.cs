using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlassPaneTopBlue : BedrockBlockData
    {
        public const string BlockId = "glass_pane_top_blue";
        public const string BlockName = "Glass Pane Top Blue";


        public GlassPaneTopBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueStainedGlassPaneTop.BlockId,
            });
        }
    }
}
