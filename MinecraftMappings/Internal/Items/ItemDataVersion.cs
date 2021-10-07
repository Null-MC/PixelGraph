namespace MinecraftMappings.Internal.Items
{
    public abstract class ItemDataVersion
    {
        public string Id {get; set;}
        public GameVersion MinVersion {get; set;}
        public GameVersion MaxVersion {get; set;}
        //public int FrameCount {get; set;} = 1;
    }
}
