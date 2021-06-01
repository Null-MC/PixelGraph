using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class Bedrock : BedrockBlockData
    {
        public const string BlockId = "bedrock";
        public const string BlockName = "Bedrock";


        public Bedrock() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Bedrock.BlockId,
            });
        }
    }
}
