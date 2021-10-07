using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class Blackstone : BedrockBlockData
    {
        public const string BlockId = "blackstone";
        public const string BlockName = "Blackstone";


        public Blackstone() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Blackstone.BlockId,
            });
        }
    }
}
