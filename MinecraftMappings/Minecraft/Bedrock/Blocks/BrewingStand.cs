using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BrewingStand : BedrockBlockData
    {
        public const string BlockId = "brewing_stand";
        public const string BlockName = "Brewing Stand";


        public BrewingStand() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BrewingStand.BlockId,
            });
        }
    }
}
