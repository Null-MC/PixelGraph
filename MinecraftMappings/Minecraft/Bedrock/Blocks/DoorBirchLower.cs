using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class DoorBirchLower : BedrockBlockData
    {
        public const string BlockId = "door_birch_lower";
        public const string BlockName = "Door Birch Lower";


        public DoorBirchLower() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.BirchDoorBottom.BlockId,
            });
        }
    }
}
