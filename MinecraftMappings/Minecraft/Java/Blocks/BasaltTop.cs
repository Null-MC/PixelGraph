using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BasaltTop : JavaBlockData
    {
        public const string BlockId = "basalt_top";
        public const string BlockName = "Basalt Top";


        public BasaltTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BasaltTop.BlockId,
            });
        }
    }
}
