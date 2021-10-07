using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BarrelSide : JavaBlockData
    {
        public const string BlockId = "barrel_side";
        public const string BlockName = "Barrel Side";


        public BarrelSide() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BarrelSide.BlockId,
                MinVersion = new GameVersion(1, 14),
            });
        }
    }
}
