using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeehiveFront : BedrockBlockData
    {
        public const string BlockId = "beehive_front";
        public const string BlockName = "Beehive Front";


        public BeehiveFront() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeehiveFront.BlockId,
            });
        }
    }
}
