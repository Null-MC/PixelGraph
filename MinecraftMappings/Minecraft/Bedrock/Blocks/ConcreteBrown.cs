using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcreteBrown : BedrockBlockData
    {
        public const string BlockId = "concrete_brown";
        public const string BlockName = "Concrete Brown";


        public ConcreteBrown() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BrownConcrete.BlockId;
            });
        }
    }
}
