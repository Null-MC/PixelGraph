using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class AnvilBase : BedrockBlockData
    {
        public const string BlockId = "anvil_base";
        public const string BlockName = "Anvil Base";


        public AnvilBase() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.Anvil.BlockId,
            });
        }
    }
}
