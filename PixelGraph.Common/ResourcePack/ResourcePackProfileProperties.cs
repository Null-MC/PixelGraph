using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackProfileProperties
    {
        public const int DefaultFormat = 5;
        public const string DefaultImageEncoding = "png";

        [YamlIgnore]
        public string LocalFile {get; set;}

        //[YamlIgnore]
        //public string LocalPath {get; set;}

        public string Edition {get; set;}

        public int? Format {get; set;}

        public string Description {get; set;}

        public string Tags {get; set;}

        public string ImageEncoding {get; set;}

        public ResourcePackOutputProperties Output {get; set;}

        //public string Sampler {get; set;}

        [YamlMember(Alias = "texture-size")]
        public int? TextureSize {get; set;}

        [YamlMember(Alias = "texture-scale")]
        public float? TextureScale {get; set;}


        public ResourcePackProfileProperties()
        {
            Output = new ResourcePackOutputProperties();
        }
    }
}
