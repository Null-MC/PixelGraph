using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class DoorAcaciaUpper : BedrockBlockData
    {
        public const string BlockId = "door_acacia_upper";
        public const string BlockName = "Door Acacia Upper";


        public DoorAcaciaUpper() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AcaciaDoorTop.BlockId,
            });
        }
    }
}
