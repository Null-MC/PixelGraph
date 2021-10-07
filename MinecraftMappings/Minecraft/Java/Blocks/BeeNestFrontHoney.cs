using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeeNestFrontHoney : JavaBlockData
    {
        public const string BlockId = "bee_nest_front_honey";
        public const string BlockName = "Bee Nest Front Honey";


        public BeeNestFrontHoney() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeeNestFrontHoney.BlockId,
            });
        }
    }
}
