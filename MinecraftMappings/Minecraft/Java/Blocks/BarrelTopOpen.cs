using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BarrelTopOpen : JavaBlockData
    {
        public const string BlockId = "barrel_top_open";
        public const string BlockName = "Barrel Top Open";


        public BarrelTopOpen() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BarrelTopOpen.BlockId,
                MinVersion = new GameVersion(1, 14),
            });
        }
    }
}
