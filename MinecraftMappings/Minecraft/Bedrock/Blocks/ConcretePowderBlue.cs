using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcretePowderBlue : BedrockBlockData
    {
        public const string BlockId = "concrete_powder_blue";
        public const string BlockName = "Concrete Powder Blue";


        public ConcretePowderBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueConcretePowder.BlockId,
            });
        }
    }
}
