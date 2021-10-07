using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeehiveFrontHoney : BedrockBlockData
    {
        public const string BlockId = "beehive_front_honey";
        public const string BlockName = "Beehive Front Honey";


        public BeehiveFrontHoney() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeehiveFrontHoney.BlockId,
            });
        }
    }
}
