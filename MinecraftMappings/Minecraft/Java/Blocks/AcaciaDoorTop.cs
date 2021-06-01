using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaDoorTop : JavaBlockData
    {
        public const string BlockId = "acacia_door_top";
        public const string BlockName = "Acacia Door Top";


        public AcaciaDoorTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.DoorAcaciaUpper.BlockId,
            });
        }
    }
}
