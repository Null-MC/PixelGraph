using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class WoolColoredBlue : BedrockBlockData
    {
        public const string BlockId = "wool_colored_blue";
        public const string BlockName = "Wool Colored Blue";


        public WoolColoredBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueWool.BlockId,
            });
        }
    }
}
