using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueConcretePowder : JavaBlockData
    {
        public const string BlockId = "blue_concrete_powder";
        public const string BlockName = "Blue Concrete Powder";


        public BlueConcretePowder() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.ConcretePowderBlue.BlockId,
            });
        }
    }
}
