using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeehiveTop : BedrockBlockData
    {
        public const string BlockId = "beehive_top";
        public const string BlockName = "Beehive Top";


        public BeehiveTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeehiveEnd.BlockId,
            });
        }
    }
}
