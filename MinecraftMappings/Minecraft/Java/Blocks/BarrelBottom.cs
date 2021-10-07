using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BarrelBottom : JavaBlockData
    {
        public const string BlockId = "barrel_bottom";
        public const string BlockName = "Barrel Bottom";


        public BarrelBottom() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BarrelBottom.BlockId,
                MinVersion = new GameVersion(1, 14),
            });
        }
    }
}
