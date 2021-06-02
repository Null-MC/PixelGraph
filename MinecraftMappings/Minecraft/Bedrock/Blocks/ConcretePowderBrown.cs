using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcretePowderBrown : BedrockBlockData
    {
        public const string BlockId = "concrete_powder_brown";
        public const string BlockName = "Concrete Powder Brown";


        public ConcretePowderBrown() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BrownConcretePowder.BlockId;
            });
        }
    }
}
