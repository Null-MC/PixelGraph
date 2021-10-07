using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BasaltSide : JavaBlockData
    {
        public const string BlockId = "basalt_side";
        public const string BlockName = "Basalt Side";


        public BasaltSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BasaltSide.BlockId,
            });
        }
    }
}
