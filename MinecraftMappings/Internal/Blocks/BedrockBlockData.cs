namespace MinecraftMappings.Internal.Blocks
{
    public abstract class BedrockBlockData : BlockData<BedrockBlockDataVersion>
    {
        protected BedrockBlockData(string name) : base(name) {}
    }

    public class BedrockBlockDataVersion : BlockDataVersion
    {
        public string MapsToJavaId {get; set;}
    }
}
