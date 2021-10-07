using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class RailActivatorPowered : BedrockBlockData
    {
        public const string BlockId = "rail_activator_powered";
        public const string BlockName = "Rail Activator Powered";


        public RailActivatorPowered() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.ActivatorRailOn.BlockId,
            });
        }
    }
}
