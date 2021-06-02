using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AncientDebrisTop : JavaBlockData
    {
        public const string BlockId = "ancient_debris_top";
        public const string BlockName = "Ancient Debris Top";


        public AncientDebrisTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.AncientDebrisTop.BlockId,
                MinVersion = new GameVersion(1, 16),
            });
        }
    }
}
