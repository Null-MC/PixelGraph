using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackProfileProperties
    {
        public const int DefaultFormat = 6;
        public const bool AutoGenerateNormalDefault = true;
        public const bool AutoGenerateOcclusionDefault = true;

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

        public bool? AutoGenerateNormal {get; set;}

        public bool? AutoGenerateOcclusion {get; set;}


        public ResourcePackProfileProperties()
        {
            Encoding = new ResourcePackOutputProperties();
        }
    }
}
