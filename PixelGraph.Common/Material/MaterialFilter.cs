using System;
using SixLabors.ImageSharp;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    //public interface IMaterialFilter
    //{
    //    public string Type {get;}
    //    public decimal? Left {get;}
    //    public decimal? Top {get;}
    //    public decimal? Width {get;}
    //    public decimal? Height {get;}
    //}

    public class MaterialFilter
    {
        [YamlMember(Order = 0)]
        public string Name {get; set;}

        [YamlMember(Order = 1)]
        public decimal? Left {get; set;}

        [YamlMember(Order = 2)]
        public decimal? Top {get; set;}

        [YamlMember(Order = 3)]
        public decimal? Width {get; set;}

        [YamlMember(Order = 4)]
        public decimal? Height {get; set;}

        [YamlMember(Order = 5)]
        public bool? WrapX {get; set;}

        [YamlMember(Order = 6)]
        public bool? WrapY {get; set;}

        [YamlMember(Order = 100)]
        public decimal? NormalNoise {get; set;}

        [YamlMember(Order = 101)]
        public decimal? NormalCurveX {get; set;}

        [YamlMember(Order = 102)]
        public decimal? NormalCurveY {get; set;}

        [YamlIgnore]
        public bool HasNormalRotation => NormalCurveX.HasValue || NormalCurveY.HasValue || NormalNoise.HasValue;


        public void GetRectangle(out RectangleF region)
        {
            region = new RectangleF {
                X = (float?)Left ?? 0f,
                Y = (float?)Top ?? 0f,
                Width = (float?)Width ?? 1f,
                Height = (float?)Height ?? 1f,
            };
        }

        public void GetRectangle(in int width, in int height, out Rectangle region)
        {
            GetRectangle(out var bounds);

            region = new Rectangle {
                X = (int)Math.Round(bounds.X * width),
                Y = (int)Math.Round(bounds.Y * height),
                Width = (int)Math.Round(bounds.Width * width),
                Height = (int)Math.Round(bounds.Height * height),
            };
        }

        //public Rectangle GetRectangle(float scale)
        //{
        //    return new Rectangle {
        //        X = (int) MathF.Ceiling((Left ?? 0) * scale),
        //        Y = (int) MathF.Ceiling((Top ?? 0) * scale),
        //        Width = (int) MathF.Ceiling((Width ?? 0) * scale),
        //        Height = (int) MathF.Ceiling((Height ?? 0) * scale),
        //    };
        //}
    }
}
