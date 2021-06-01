using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueStainedGlassPaneTop : JavaBlockData
    {
        public const string BlockId = "blue_stained_glass_pane_top";
        public const string BlockName = "Blue Stained Glass Pane Top";


        public BlueStainedGlassPaneTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.GlassPaneTopBlue.BlockId,
            });
        }
    }
}
