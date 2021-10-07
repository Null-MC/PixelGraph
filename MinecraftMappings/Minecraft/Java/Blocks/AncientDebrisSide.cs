using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AncientDebrisSide : JavaBlockData
    {
        public const string BlockId = "ancient_debris_side";
        public const string BlockName = "Ancient Debris Side";


        public AncientDebrisSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.AncientDebrisSide.BlockId,
                MinVersion = new GameVersion(1, 16),
            });
        }
    }
}
