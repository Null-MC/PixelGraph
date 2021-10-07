using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class SaplingAcacia : BedrockBlockData
    {
        public const string BlockId = "sapling_acacia";
        public const string BlockName = "Sapling Acacia";


        public SaplingAcacia() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AcaciaSapling.BlockId,
            });
        }
    }
}
