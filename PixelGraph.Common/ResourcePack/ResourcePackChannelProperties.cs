using PixelGraph.Common.Textures;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackChannelProperties
    {
        [YamlIgnore]
        public string ID {get;}

        public string Texture {get; set;}
        public ColorChannel? Color {get; set;}
        public string Sampler {get; set;}
        public byte? MinValue {get; set;}
        public byte? MaxValue {get; set;}
        public short? Shift {get; set;}
        public float? Power {get; set;}
        public bool? Invert {get; set;}


        public ResourcePackChannelProperties(string id)
        {
            ID = id;
        }
    }
}
