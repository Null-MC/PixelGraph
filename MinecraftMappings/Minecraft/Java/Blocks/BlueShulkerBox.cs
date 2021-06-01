using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueShulkerBox : JavaBlockData
    {
        public const string BlockId = "blue_shulker_box";
        public const string BlockName = "Blue Shulker Box";


        public BlueShulkerBox() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.ShulkerTopBlue.BlockId,
            });
        }
    }
}
