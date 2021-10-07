using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeeNestFront : JavaBlockData
    {
        public const string BlockId = "bee_nest_front";
        public const string BlockName = "Bee Nest Front";


        public BeeNestFront() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeeNestFront.BlockId,
            });
        }
    }
}
