using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaDoorBottom : JavaBlockData
    {
        public const string BlockId = "acacia_door_bottom";
        public const string BlockName = "Acacia Door Bottom";


        public AcaciaDoorBottom() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.DoorAcaciaLower.BlockId,
            });
        }
    }
}
