using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BarrelTopOpen : BedrockBlockData
    {
        public const string BlockId = "barrel_top_open";
        public const string BlockName = "Barrel Top Open";


        public BarrelTopOpen() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToJavaId = Java.Blocks.BarrelTopOpen.BlockId;
                version.MinVersion = new GameVersion(1, 9);
            });
        }
    }
}
