using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BasaltSide : BedrockBlockData
    {
        public const string BlockId = "basalt_side";
        public const string BlockName = "Basalt Side";


        public BasaltSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BasaltSide.BlockId,
            });
        }
    }
}
