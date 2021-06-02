using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft
{
    public static class Minecraft
    {
        public static MinecraftJava Java {get;} = new MinecraftJava();

        public static MinecraftBedrock Bedrock {get;} = new MinecraftBedrock();
    }

    public class MinecraftJava : MinecraftGameEdition<JavaBlockData, JavaBlockDataVersion, JavaEntityData, JavaEntityDataVersion> {}

    public class MinecraftBedrock : MinecraftGameEdition<BedrockBlockData, BedrockBlockDataVersion, BedrockEntityData, BedrockEntityDataVersion> {}
}
