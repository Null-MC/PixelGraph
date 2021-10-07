using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueStainedGlass : JavaBlockData
    {
        public const string BlockId = "blue_stained_glass";
        public const string BlockName = "Blue Stained Glass";


        public BlueStainedGlass() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.GlassBlue.BlockId,
            });
        }
    }
}
