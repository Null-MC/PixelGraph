using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material.Filters
{
    public class MaterialNormalCurveFilter : MaterialFilterBase
    {
        public const string FilterType = "normal-curve";

        [YamlMember(Order = 100)]
        public decimal? AngleX {get; set;}

        [YamlMember(Order = 101)]
        public decimal? AngleY {get; set;}
    }
}
