using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class PlanksBirch : BedrockBlockData
    {
        public const string BlockId = "planks_birch";
        public const string BlockName = "Planks Birch";


        public PlanksBirch() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BirchPlanks.BlockId,
            });
        }
    }
}
