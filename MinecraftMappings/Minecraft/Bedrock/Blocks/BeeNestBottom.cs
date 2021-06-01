using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeeNestBottom : BedrockBlockData
    {
        public const string BlockId = "bee_nest_bottom";
        public const string BlockName = "Bee Nest Bottom";


        public BeeNestBottom() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeeNestBottom.BlockId,
            });
        }
    }
}
