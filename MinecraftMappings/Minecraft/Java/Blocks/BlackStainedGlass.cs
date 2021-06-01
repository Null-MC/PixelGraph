using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackStainedGlass : JavaBlockData
    {
        public const string BlockId = "black_stained_glass";
        public const string BlockName = "Black Stained Glass";


        public BlackStainedGlass() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.GlassBlack.BlockId,
            });
        }
    }
}
