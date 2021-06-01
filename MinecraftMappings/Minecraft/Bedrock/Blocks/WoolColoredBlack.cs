using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class WoolColoredBlack : BedrockBlockData
    {
        public const string BlockId = "wool_colored_black";
        public const string BlockName = "Wool Colored Black";


        public WoolColoredBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackWool.BlockId,
            });
        }
    }
}
