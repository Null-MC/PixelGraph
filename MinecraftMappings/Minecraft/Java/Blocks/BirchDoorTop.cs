using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchDoorTop : JavaBlockData
    {
        public const string BlockId = "birch_door_top";
        public const string BlockName = "Birch Door Top";


        public BirchDoorTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.DoorBirchUpper.BlockId,
            });
        }
    }
}
