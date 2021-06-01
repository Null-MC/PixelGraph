using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class AncientDebrisSide : BedrockBlockData
    {
        public const string BlockId = "ancient_debris_side";
        public const string BlockName = "Ancient Debris Side";


        public AncientDebrisSide() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AncientDebrisSide.BlockId,
                MinVersion = "1.16.0",
            });
        }
    }
}
