using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BarrelTopOpen : BedrockBlockData
    {
        public const string BlockId = "barrel_top_open";
        public const string BlockName = "Barrel Top Open";


        public BarrelTopOpen() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BarrelTopOpen.BlockId,
                MinVersion = "1.9.0",
            });
        }
    }
}
