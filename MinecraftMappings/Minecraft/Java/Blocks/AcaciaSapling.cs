using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaSapling : JavaBlockData
    {
        public const string BlockId = "acacia_sapling";
        public const string BlockName = "Acacia Sapling";


        public AcaciaSapling() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.SaplingAcacia.BlockId,
            });
        }
    }
}
