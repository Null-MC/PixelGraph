using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchLogTop : JavaBlockData
    {
        public const string BlockId = "birch_log_top";
        public const string BlockName = "Birch Log Top";


        public BirchLogTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.LogBirchTop.BlockId,
            });
        }
    }
}
