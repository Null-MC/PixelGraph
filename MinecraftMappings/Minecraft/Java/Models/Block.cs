using MinecraftMappings.Internal.Models;

namespace MinecraftMappings.Minecraft.Java.Models
{
    public class Block : JavaModelData
    {
        public Block() : base("Block")
        {
            AddVersion("block", "1.0.0");
        }
    }
}
