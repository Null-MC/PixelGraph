using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BoneBlockTop : JavaBlockData
    {
        public const string BlockId = "bone_block_top";
        public const string BlockName = "Bone Block Top";


        public BoneBlockTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BoneBlockTop.BlockId,
            });
        }
    }
}
