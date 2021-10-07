using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BoneBlockSide : JavaBlockData
    {
        public const string BlockId = "bone_block_side";
        public const string BlockName = "Bone Block Side";


        public BoneBlockSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BoneBlockSide.BlockId,
            });
        }
    }
}
