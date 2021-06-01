using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Bedrock : JavaBlockData
    {
        public const string BlockId = "bedrock";
        public const string BlockName = "Bedrock";


        public Bedrock() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.Bedrock.BlockId,
            });
        }
    }
}
