using PixelGraph.Common.Encoding;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialNormalProperties
    {
        public const float DefaultStrength = 1f;
        public const float DefaultNoise = 0f;

        public TextureEncoding Input {get; set;}
        public string Texture {get; set;}
        
        [YamlMember(Alias = "value-x", ApplyNamingConventions = false)]
        public byte? ValueX {get; set;}

        [YamlMember(Alias = "value-y", ApplyNamingConventions = false)]
        public byte? ValueY {get; set;}

        [YamlMember(Alias = "value-z", ApplyNamingConventions = false)]
        public byte? ValueZ {get; set;}

        public decimal? Strength {get; set;}
        public decimal? Noise {get; set;}
        public decimal? CurveX {get; set;}
        public decimal? CurveY {get; set;}
    }
}
