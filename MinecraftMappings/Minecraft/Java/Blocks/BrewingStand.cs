using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BrewingStand : JavaBlockData
    {
        public const string BlockId = "brewing_stand";
        public const string BlockName = "Brewing Stand";


        public BrewingStand() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BrewingStand.BlockId,
            });
        }
    }
}
