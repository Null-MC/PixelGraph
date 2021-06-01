using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class LogAcaciaTop : BedrockBlockData
    {
        public const string BlockId = "log_acacia_top";
        public const string BlockName = "Log Acacia Top";


        public LogAcaciaTop() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AcaciaLogTop.BlockId,
            });
        }
    }
}
