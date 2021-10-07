using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Andesite : JavaBlockData
    {
        public const string BlockId = "andesite";
        public const string BlockName = "Andesite";


        public Andesite() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.StoneAndesite.BlockId,
            });
        }
    }
}
