using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BrownMushroomBlock : JavaBlockData
    {
        public const string BlockId = "brown_mushroom_block";
        public const string BlockName = "Brown Mushroom Block";


        public BrownMushroomBlock() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.MushroomBlockSkinBrown.BlockId;
            });
        }
    }
}
