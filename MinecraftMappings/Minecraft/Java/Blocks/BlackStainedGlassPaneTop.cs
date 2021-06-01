using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackStainedGlassPaneTop : JavaBlockData
    {
        public const string BlockId = "black_stained_glass_pane_top";
        public const string BlockName = "Black Stained Glass Pane Top";


        public BlackStainedGlassPaneTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.GlassPaneTopBlack.BlockId,
            });
        }
    }
}
