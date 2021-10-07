using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class ActivatorRail : JavaBlockData
    {
        public const string BlockId = "activator_rail";
        public const string BlockName = "Activator Rail";


        public ActivatorRail() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.RailActivator.BlockId,
            });
        }
    }
}
