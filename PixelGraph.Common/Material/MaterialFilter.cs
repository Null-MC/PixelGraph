using SixLabors.ImageSharp;
using System;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
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
        public bool? Tile {get; set;}

        [YamlMember(Order = 100)]
        public decimal? NormalNoise {get; set;}

        [YamlMember(Order = 101)]
        public decimal? NormalCurveX {get; set;}

        [YamlMember(Order = 102)]
        public decimal? NormalCurveLeft {get; set;}

        [YamlMember(Order = 103)]
        public decimal? NormalCurveRight {get; set;}

        [YamlMember(Order = 104)]
        public decimal? NormalCurveY {get; set;}

        [YamlMember(Order = 105)]
        public decimal? NormalCurveTop {get; set;}

        [YamlMember(Order = 106)]
        public decimal? NormalCurveBottom {get; set;}

        [YamlMember(Order = 107)]
        public decimal? NormalRadiusX {get; set;}

        [YamlMember(Order = 108)]
        public decimal? NormalRadiusLeft {get; set;}

        [YamlMember(Order = 109)]
        public decimal? NormalRadiusRight {get; set;}

        [YamlMember(Order = 110)]
        public decimal? NormalRadiusY {get; set;}

        [YamlMember(Order = 111)]
        public decimal? NormalRadiusTop {get; set;}

        [YamlMember(Order = 112)]
        public decimal? NormalRadiusBottom {get; set;}

        [YamlIgnore]
        public bool HasNormalRotationLeft => Math.Abs(NormalCurveLeft ?? NormalCurveX ?? 0m) > 0m && (NormalRadiusLeft ?? NormalRadiusX ?? 1m) > 0m;

        [YamlIgnore]
        public bool HasNormalRotationRight => Math.Abs(NormalCurveRight ?? NormalCurveX ?? 0m) > 0m && (NormalRadiusRight ?? NormalRadiusX ?? 1m) > 0m;

        [YamlIgnore]
        public bool HasNormalRotationTop => Math.Abs(NormalCurveTop ?? NormalCurveY ?? 0m) > 0m && (NormalRadiusTop ?? NormalRadiusY ?? 1m) > 0m;

        [YamlIgnore]
        public bool HasNormalRotationBottom => Math.Abs(NormalCurveBottom ?? NormalCurveY ?? 0m) > 0m && (NormalRadiusBottom ?? NormalRadiusY ?? 1m) > 0m;

        [YamlIgnore]
        public bool HasNormalRotation => HasNormalRotationLeft || HasNormalRotationRight || HasNormalRotationTop || HasNormalRotationBottom || NormalNoise.HasValue;


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
                X = (int)(bounds.X * width + 0.5f),
                Y = (int)(bounds.Y * height + 0.5f),
                Width = (int)(bounds.Width * width + 0.5f),
                Height = (int)(bounds.Height * height + 0.5f),
            };
        }
        
        public decimal? GetNormalCurveTop() => GetCurveValue(NormalCurveTop, NormalCurveY);
        public decimal? GetNormalCurveBottom() => GetCurveValue(NormalCurveBottom, NormalCurveY);
        public decimal? GetNormalCurveLeft() => GetCurveValue(NormalCurveLeft, NormalCurveX);
        public decimal? GetNormalCurveRight() => GetCurveValue(NormalCurveRight, NormalCurveX);

        public decimal? GetNormalRadiusTop() => NormalRadiusTop ?? NormalRadiusY;
        public decimal? GetNormalRadiusBottom() => NormalRadiusBottom ?? NormalRadiusY;
        public decimal? GetNormalRadiusLeft() => NormalRadiusLeft ?? NormalRadiusX;
        public decimal? GetNormalRadiusRight() => NormalRadiusRight ?? NormalRadiusX;

        private static decimal? GetCurveValue(in decimal? sideValue, in decimal? axisValue)
        {
            if (sideValue.HasValue) return sideValue.Value;
            if (axisValue.HasValue) return axisValue.Value / 2;
            return null;
        }
    }
}
