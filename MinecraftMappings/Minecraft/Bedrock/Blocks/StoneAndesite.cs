using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class StoneAndesite : BedrockBlockData
    {
        public const string BlockId = "stone_andesite";
        public const string BlockName = "Stone Andesite";


        public StoneAndesite() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Andesite.BlockId,
            });
        }
    }
}
