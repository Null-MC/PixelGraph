using PixelGraph.Common.ResourcePack;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialNormalProperties
    {
        public const float DefaultStrength = 1f;
        public const float DefaultNoise = 0f;

        public string Texture {get; set;}
        public decimal? Strength {get; set;}
        public decimal? Noise {get; set;}
        public decimal? CurveX {get; set;}
        public decimal? CurveY {get; set;}
        
        public ResourcePackNormalXChannelProperties InputX {get; set;}

        [YamlMember(Alias = "value-x", ApplyNamingConventions = false)]
        public byte? ValueX {get; set;}

        public ResourcePackNormalYChannelProperties InputY {get; set;}

        [YamlMember(Alias = "value-y", ApplyNamingConventions = false)]
        public byte? ValueY {get; set;}

        public ResourcePackNormalZChannelProperties InputZ {get; set;}

        [YamlMember(Alias = "value-z", ApplyNamingConventions = false)]
        public byte? ValueZ {get; set;}
    }
}
