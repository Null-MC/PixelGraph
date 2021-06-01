using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchDoorBottom : JavaBlockData
    {
        public const string BlockId = "birch_door_bottom";
        public const string BlockName = "Birch Door Bottom";


        public BirchDoorBottom() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.DoorBirchLower.BlockId,
            });
        }
    }
}
