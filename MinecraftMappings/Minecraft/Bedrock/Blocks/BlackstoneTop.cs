using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BlackstoneTop : BedrockBlockData
    {
        public const string BlockId = "blackstone_top";
        public const string BlockName = "Blackstone Top";


        public BlackstoneTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackstoneTop.BlockId,
            });
        }
    }
}
