using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlassBlue : BedrockBlockData
    {
        public const string BlockId = "glass_blue";
        public const string BlockName = "Glass Blue";


        public GlassBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueStainedGlass.BlockId,
            });
        }
    }
}
