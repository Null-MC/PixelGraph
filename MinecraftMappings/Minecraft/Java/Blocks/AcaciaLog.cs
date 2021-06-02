using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaLog : JavaBlockData
    {
        public const string BlockId = "acacia_log";
        public const string BlockName = "Acacia Log";


        public AcaciaLog() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.LogAcacia.BlockId;
            });
        }
    }
}
