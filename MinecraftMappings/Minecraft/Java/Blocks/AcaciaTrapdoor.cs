using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaTrapdoor : JavaBlockData
    {
        public const string BlockId = "acacia_trapdoor";
        public const string BlockName = "Acacia Trapdoor";


        public AcaciaTrapdoor() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.AcaciaTrapdoor.BlockId,
            });
        }
    }
}
