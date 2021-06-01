using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchLog : JavaBlockData
    {
        public const string BlockId = "birch_log";
        public const string BlockName = "Birch Log";


        public BirchLog() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.LogBirch.BlockId,
            });
        }
    }
}
