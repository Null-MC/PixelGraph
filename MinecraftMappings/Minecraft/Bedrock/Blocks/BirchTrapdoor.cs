using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BirchTrapdoor : BedrockBlockData
    {
        public const string BlockId = "birch_trapdoor";
        public const string BlockName = "Birch Trapdoor";


        public BirchTrapdoor() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BirchTrapdoor.BlockId,
            });
        }
    }
}
