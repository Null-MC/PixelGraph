using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcretePowderBlack : BedrockBlockData
    {
        public const string BlockId = "concrete_powder_black";
        public const string BlockName = "Concrete Powder Black";


        public ConcretePowderBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackConcretePowder.BlockId,
            });
        }
    }
}
