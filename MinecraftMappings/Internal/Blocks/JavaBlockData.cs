namespace MinecraftMappings.Internal.Blocks
{
    public abstract class JavaBlockData : BlockData<JavaBlockDataVersion>
    {
        protected JavaBlockData(string name) : base(name) {}
    }

    public class JavaBlockDataVersion : BlockDataVersion
    {
        public string MapsToBedrockId {get; set;}
    }
}
