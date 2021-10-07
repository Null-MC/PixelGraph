using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class Brick : BedrockBlockData
    {
        public const string BlockId = "brick";
        public const string BlockName = "Brick";


        public Brick() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Bricks.BlockId,
            });
        }
    }
}
