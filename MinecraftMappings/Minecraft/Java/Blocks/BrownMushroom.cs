using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BrownMushroom : JavaBlockData
    {
        public const string BlockId = "brown_mushroom";
        public const string BlockName = "Brown Mushroom";


        public BrownMushroom() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.MushroomBrown.BlockId;
            });
        }
    }
}
