using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class LogAcacia : BedrockBlockData
    {
        public const string BlockId = "log_acacia";
        public const string BlockName = "Log Acacia";


        public LogAcacia() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AcaciaLog.BlockId,
            });
        }
    }
}
