using SixLabors.ImageSharp;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialPart
    {
        [YamlMember(Order = 0)]
        public string Name {get; set;}

        [YamlMember(Order = 1)]
        public int? Left {get; set;}

        [YamlMember(Order = 2)]
        public int? Top {get; set;}

        [YamlMember(Order = 3)]
        public int? Width {get; set;}

        [YamlMember(Order = 4)]
        public int? Height {get; set;}


        public Rectangle GetRectangle()
        {
            return new Rectangle(Left ?? 0, Top ?? 0, Width ?? 0, Height ?? 0);
        }
    }
}
