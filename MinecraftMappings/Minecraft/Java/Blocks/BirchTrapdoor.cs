using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BirchTrapdoor : JavaBlockData
    {
        public const string BlockId = "birch_trapdoor";
        public const string BlockName = "Birch Trapdoor";


        public BirchTrapdoor() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BirchTrapdoor.BlockId,
            });
        }
    }
}
