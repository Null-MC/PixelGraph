using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class ActivatorRailOn : JavaBlockData
    {
        public const string BlockId = "activator_rail_on";
        public const string BlockName = "Activator Rail On";


        public ActivatorRailOn() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.RailActivatorPowered.BlockId,
            });
        }
    }
}
