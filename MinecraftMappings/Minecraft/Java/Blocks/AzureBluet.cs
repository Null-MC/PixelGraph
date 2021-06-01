using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AzureBluet : JavaBlockData
    {
        public const string BlockId = "azure_bluet";
        public const string BlockName = "Azure Bluet";


        public AzureBluet() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.FlowerHoustonia.BlockId,
            });
        }
    }
}
