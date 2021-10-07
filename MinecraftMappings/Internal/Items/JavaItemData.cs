namespace MinecraftMappings.Internal.Items
{
    public abstract class JavaItemData : ItemData<JavaItemDataVersion>
    {
        protected JavaItemData(string name) : base(name) {}
    }

    public class JavaItemDataVersion : ItemDataVersion
    {
        //public string MapsToBedrockId {get; set;}
    }
}
