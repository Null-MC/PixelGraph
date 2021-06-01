using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Anvil : JavaBlockData
    {
        public const string BlockId = "anvil";
        public const string BlockName = "Anvil";


        public Anvil() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.AnvilBase.BlockId,
            });
        }
    }
}
