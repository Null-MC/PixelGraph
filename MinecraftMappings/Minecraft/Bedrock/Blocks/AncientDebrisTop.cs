using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class AncientDebrisTop : BedrockBlockData
    {
        public const string BlockId = "ancient_debris_top";
        public const string BlockName = "Ancient Debris Top";


        public AncientDebrisTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AncientDebrisTop.BlockId,
                MinVersion = "1.16.0",
            });
        }
    }
}
