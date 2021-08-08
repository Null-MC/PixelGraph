namespace MinecraftMappings.Internal
{
    public abstract class BedrockItemData : ItemData<BedrockItemDataVersion>
    {
        protected BedrockItemData(string name) : base(name) {}
    }

    public class BedrockItemDataVersion : ItemDataVersion
    {
        //public string MapsToJavaId {get; set;}
    }
}
