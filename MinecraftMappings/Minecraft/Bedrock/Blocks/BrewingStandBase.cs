using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BrewingStandBase : BedrockBlockData
    {
        public const string BlockId = "brewing_stand_base";
        public const string BlockName = "Brewing Stand Base";


        public BrewingStandBase() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BrewingStandBase.BlockId,
            });
        }
    }
}
