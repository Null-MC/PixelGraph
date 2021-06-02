using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BlueIce : JavaBlockData
    {
        public const string BlockId = "blue_ice";
        public const string BlockName = "Blue Ice";


        public BlueIce() : base(BlockName)
        {
            AddVersion(BlockId, version => {
                version.MapsToBedrockId = BedrockBlocks.BlueIce.BlockId;
            });
        }
    }
}
