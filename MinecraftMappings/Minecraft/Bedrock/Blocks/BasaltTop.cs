using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BasaltTop : BedrockBlockData
    {
        public const string BlockId = "basalt_top";
        public const string BlockName = "Basalt Top";


        public BasaltTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BasaltTop.BlockId,
            });
        }
    }
}
