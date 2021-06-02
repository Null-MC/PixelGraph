using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class BarrelTop : BedrockBlockData
    {
        public const string BlockId = "barrel_top";
        public const string BlockName = "Barrel Top";


        public BarrelTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BarrelTop.BlockId,
                MinVersion = new GameVersion(1, 9),
            });
        }
    }
}
