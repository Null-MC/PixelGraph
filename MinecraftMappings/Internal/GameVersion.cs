namespace MinecraftMappings.Internal
{
    public class GameVersion
    {
        public int Major {get; set;}
        public int Minor {get; set;}
        public int Revision {get; set;}


        public GameVersion() {}

        public GameVersion(int major, int minor, int revision = 0)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }
    }
}
