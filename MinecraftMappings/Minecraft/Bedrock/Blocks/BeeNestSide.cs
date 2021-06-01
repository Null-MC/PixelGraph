using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeeNestSide : BedrockBlockData
    {
        public const string BlockId = "bee_nest_side";
        public const string BlockName = "Bee Nest Side";


        public BeeNestSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeeNestSide.BlockId,
            });
        }
    }
}
