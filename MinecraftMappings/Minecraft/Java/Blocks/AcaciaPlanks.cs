using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaPlanks : JavaBlockData
    {
        public const string BlockId = "acacia_planks";
        public const string BlockName = "Acacia Planks";


        public AcaciaPlanks() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.PlanksAcacia.BlockId,
            });
        }
    }
}
