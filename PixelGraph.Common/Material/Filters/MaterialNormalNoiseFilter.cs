using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material.Filters
{
    public class MaterialNormalNoiseFilter : MaterialFilterBase
    {
        public const string FilterType = "normal-noise";

        [YamlMember(Order = 100)]
        public decimal? Angle {get; set;}
    }
}
