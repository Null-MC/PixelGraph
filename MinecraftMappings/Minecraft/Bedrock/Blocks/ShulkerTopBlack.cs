using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ShulkerTopBlack : BedrockBlockData
    {
        public const string BlockId = "shulker_top_black";
        public const string BlockName = "Shulker Top Black";


        public ShulkerTopBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackShulkerBox.BlockId,
            });
        }
    }
}
