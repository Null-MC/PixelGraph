using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class MushroomBlockSkinBrown : BedrockBlockData
    {
        public const string BlockId = "mushroom_block_skin_brown";
        public const string BlockName = "Mushroom Block Skin Brown";


        public MushroomBlockSkinBrown() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BrownMushroomBlock.BlockId;
            });
        }
    }
}
