using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BrownConcrete : JavaBlockData
    {
        public const string BlockId = "brown_concrete";
        public const string BlockName = "Brown Concrete";


        public BrownConcrete() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.ConcreteBrown.BlockId;
                version.MinVersion = new GameVersion(1, 12);
            });
        }
    }
}
