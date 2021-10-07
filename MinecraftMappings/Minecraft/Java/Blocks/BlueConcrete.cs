using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueConcrete : JavaBlockData
    {
        public const string BlockId = "blue_concrete";
        public const string BlockName = "Blue Concrete";


        public BlueConcrete() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.ConcreteBlue.BlockId;
                version.MinVersion = new GameVersion(1, 12);
            });
        }
    }
}
