using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackProfileProperties
    {
        public const int DefaultFormat = 6;

        [YamlIgnore]
        public string LocalFile {get; set;}

        public string Edition {get; set;}

        public int? Format {get; set;}

        public string Description {get; set;}

        public string Tags {get; set;}

        public ResourcePackOutputProperties Encoding {get; set;}

        public int? TextureSize {get; set;}

        public float? TextureScale {get; set;}

        //public int? BlockSize {get; set;}


        public ResourcePackProfileProperties()
        {
            Encoding = new ResourcePackOutputProperties();
        }
    }
}
