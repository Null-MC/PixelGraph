using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Bedrock.Blocks
{
    public class AcaciaTrapdoor : BedrockBlockData
    {
        public const string BlockId = "acacia_trapdoor";
        public const string BlockName = "Acacia Trapdoor";


        public AcaciaTrapdoor() : base(BlockName)
        {
            Versions.Add(new BedrockBlockDataVersion {
                Id = BlockId,
                MapsToJavaId = Java.Blocks.AcaciaTrapdoor.BlockId,
            });
        }
    }
}
