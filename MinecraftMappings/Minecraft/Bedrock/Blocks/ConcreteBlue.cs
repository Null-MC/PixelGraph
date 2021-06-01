using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class ConcreteBlue : BedrockBlockData
    {
        public const string BlockId = "concrete_blue";
        public const string BlockName = "Concrete Blue";


        public ConcreteBlue() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BlueConcrete.BlockId,
            });
        }
    }
}
