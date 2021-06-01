using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcreteBlack : BedrockBlockData
    {
        public const string BlockId = "concrete_black";
        public const string BlockName = "Concrete Black";


        public ConcreteBlack() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlackConcrete.BlockId,
            });
        }
    }
}
