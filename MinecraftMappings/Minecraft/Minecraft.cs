using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Blocks;
using MinecraftMappings.Internal.Entities;
using MinecraftMappings.Internal.Items;
using MinecraftMappings.Internal.Models;

namespace MinecraftMappings.Minecraft
{
    public static class Minecraft
    {
        public static MinecraftJava Java {get;} = new MinecraftJava();

        public static MinecraftBedrock Bedrock {get;} = new MinecraftBedrock();
    }

    public class MinecraftJava : MinecraftGameEdition<JavaBlockData, JavaItemData, JavaEntityData, JavaModelData> {}

    public class MinecraftBedrock : MinecraftGameEdition<BedrockBlockData, BedrockItemData, BedrockEntityData, BedrockModelData> {}
}
