using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class GlassBlack : BedrockBlockData
    {
        public const string BlockId = "glass_black";
        public const string BlockName = "Glass Black";


        public GlassBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackStainedGlass.BlockId,
            });
        }
    }
}
