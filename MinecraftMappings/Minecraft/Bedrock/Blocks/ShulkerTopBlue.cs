using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ShulkerTopBlue : BedrockBlockData
    {
        public const string BlockId = "shulker_top_blue";
        public const string BlockName = "Shulker Top Blue";


        public ShulkerTopBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueShulkerBox.BlockId,
            });
        }
    }
}
