using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlackConcrete : JavaBlockData
    {
        public const string BlockId = "black_concrete";
        public const string BlockName = "Black Concrete";


        public BlackConcrete() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MinVersion = new GameVersion(1, 12),
                MapsToBedrockId = BedrockBlocks.ConcreteBlack.BlockId,
            });
        }
    }
}
