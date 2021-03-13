using SixLabors.ImageSharp;
using System;
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
            return new Rectangle {
                X = Left ?? 0,
                Y = Top ?? 0,
                Width = Width ?? 0,
                Height = Height ?? 0,
            };
        }

        public Rectangle GetRectangle(float scale)
        {
            return new Rectangle {
                X = (int) MathF.Ceiling((Left ?? 0) * scale),
                Y = (int) MathF.Ceiling((Top ?? 0) * scale),
                Width = (int) MathF.Ceiling((Width ?? 0) * scale),
                Height = (int) MathF.Ceiling((Height ?? 0) * scale),
            };
        }
    }
}
