using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueConcrete : JavaBlockData
    {
        public const string BlockId = "blue_concrete";
        public const string BlockName = "Blue Concrete";


        public BlueConcrete() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.ConcreteBlue.BlockId,
                MinVersion = "1.12",
            });
        }
    }
}
