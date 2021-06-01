using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class BeeNestTop : JavaBlockData
    {
        public const string BlockId = "bee_nest_top";
        public const string BlockName = "Bee Nest Top";


        public BeeNestTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.BeeNestTop.BlockId,
            });
        }
    }
}
