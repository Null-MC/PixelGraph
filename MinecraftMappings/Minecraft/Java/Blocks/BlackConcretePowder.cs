using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackConcretePowder : JavaBlockData
    {
        public const string BlockId = "black_concrete_powder";
        public const string BlockName = "Black Concrete Powder";


        public BlackConcretePowder() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.ConcretePowderBlack.BlockId,
            });
        }
    }
}
