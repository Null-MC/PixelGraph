using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class Beacon : JavaBlockData
    {
        public const string BlockId = "beacon";
        public const string BlockName = "Beacon";


        public Beacon() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.Beacon.BlockId,
            });
        }
    }
}
