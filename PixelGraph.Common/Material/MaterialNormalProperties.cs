using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialNormalProperties
    {
        public const float DefaultStrength = 1f;
        public const float DefaultNoise = 0f;
        public const NormalMapFilters DefaultFilter = NormalMapFilters.Sobel3;

        public string Texture {get; set;}
        public decimal? Strength {get; set;}
        public decimal? Noise {get; set;}
        public decimal? CurveX {get; set;}
        public decimal? CurveY {get; set;}
        public NormalMapFilters? Filter {get; set;}
        
        public ResourcePackNormalXChannelProperties InputX {get; set;}

        [YamlMember(Alias = "value-x", ApplyNamingConventions = false)]
        public decimal? ValueX {get; set;}

        public ResourcePackNormalYChannelProperties InputY {get; set;}

        [YamlMember(Alias = "value-y", ApplyNamingConventions = false)]
        public decimal? ValueY {get; set;}

        public ResourcePackNormalZChannelProperties InputZ {get; set;}

        [YamlMember(Alias = "value-z", ApplyNamingConventions = false)]
        public decimal? ValueZ {get; set;}
    }
}
