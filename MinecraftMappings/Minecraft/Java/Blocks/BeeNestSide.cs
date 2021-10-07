using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeeNestSide : JavaBlockData
    {
        public const string BlockId = "bee_nest_side";
        public const string BlockName = "Bee Nest Side";


        public BeeNestSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeeNestSide.BlockId,
            });
        }
    }
}
