using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BeeNestFrontHoney : BedrockBlockData
    {
        public const string BlockId = "bee_nest_front_honey";
        public const string BlockName = "Bee Nest Front Honey";


        public BeeNestFrontHoney() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BeeNestFrontHoney.BlockId,
            });
        }
    }
}
