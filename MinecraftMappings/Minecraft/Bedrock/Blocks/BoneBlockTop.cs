using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BoneBlockTop : BedrockBlockData
    {
        public const string BlockId = "bone_block_top";
        public const string BlockName = "Bone Block Top";


        public BoneBlockTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BoneBlockTop.BlockId,
            });
        }
    }
}
