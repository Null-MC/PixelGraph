using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BarrelBottom : BedrockBlockData
    {
        public const string BlockId = "barrel_bottom";
        public const string BlockName = "Barrel Bottom";


        public BarrelBottom() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BarrelBottom.BlockId,
                MinVersion = new GameVersion(1, 9),
            });
        }
    }
}
