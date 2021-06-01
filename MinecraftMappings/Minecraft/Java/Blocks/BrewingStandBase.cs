using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BrewingStandBase : JavaBlockData
    {
        public const string BlockId = "brewing_stand_base";
        public const string BlockName = "Brewing Stand Base";


        public BrewingStandBase() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BrewingStandBase.BlockId,
            });
        }
    }
}
