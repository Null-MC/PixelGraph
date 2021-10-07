using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BoneBlockSide : BedrockBlockData
    {
        public const string BlockId = "bone_block_side";
        public const string BlockName = "Bone Block Side";


        public BoneBlockSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BoneBlockSide.BlockId,
            });
        }
    }
}
