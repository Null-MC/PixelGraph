using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeeNestTop : BedrockBlockData
    {
        public const string BlockId = "bee_nest_top";
        public const string BlockName = "Bee Nest Top";


        public BeeNestTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeeNestTop.BlockId,
            });
        }
    }
}
