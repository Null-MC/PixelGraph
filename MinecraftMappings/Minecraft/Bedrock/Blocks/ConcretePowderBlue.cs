using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcretePowderBlue : BedrockBlockData
    {
        public const string BlockId = "concrete_powder_blue";
        public const string BlockName = "Concrete Powder Blue";


        public ConcretePowderBlue() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BlueConcretePowder.BlockId;
            });
        }
    }
}
