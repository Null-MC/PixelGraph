using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BambooStem : BedrockBlockData
    {
        public const string BlockId = "bamboo_stem";
        public const string BlockName = "Bamboo Stem";


        public BambooStem() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BambooStalk.BlockId,
            });
        }
    }
}
