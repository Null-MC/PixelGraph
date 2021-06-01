using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackShulkerBox : JavaBlockData
    {
        public const string BlockId = "black_shulker_box";
        public const string BlockName = "Black Shulker Box";


        public BlackShulkerBox() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.ShulkerTopBlack.BlockId,
            });
        }
    }
}
