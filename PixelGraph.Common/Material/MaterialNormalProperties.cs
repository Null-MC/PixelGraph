using PixelGraph.Common.ResourcePack;
using System;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialNormalProperties
    {
        public const decimal DefaultStrength = 1.0m;
        //public const string DefaultMethod = NormalMapMethod.Sobel3;

        public string Texture {get; set;}
        public decimal? Strength {get; set;}
        public string Method {get; set;}
        
        public ResourcePackNormalXChannelProperties InputX {get; set;}

        [YamlMember(Alias = "value-x", ApplyNamingConventions = false)]
        public decimal? ValueX {get; set;}

        public ResourcePackNormalYChannelProperties InputY {get; set;}

        [YamlMember(Alias = "value-y", ApplyNamingConventions = false)]
        public decimal? ValueY {get; set;}

        public ResourcePackNormalZChannelProperties InputZ {get; set;}

        [YamlMember(Alias = "value-z", ApplyNamingConventions = false)]
        public decimal? ValueZ {get; set;}


        public bool HasAnyData()
        {
            if (InputX?.HasAnyData() ?? false) return true;
            if (InputY?.HasAnyData() ?? false) return true;
            if (InputZ?.HasAnyData() ?? false) return true;

            if (Texture != null) return true;
            if (Strength.HasValue) return true;
            if (Method != null) return true;

            if (ValueX.HasValue) return true;
            if (ValueY.HasValue) return true;
            if (ValueZ.HasValue) return true;

            if (Noise.HasValue) return true;
            if (CurveX.HasValue) return true;
            if (CurveLeft.HasValue) return true;
            if (CurveRight.HasValue) return true;
            if (CurveY.HasValue) return true;
            if (CurveTop.HasValue) return true;
            if (CurveBottom.HasValue) return true;
            if (RadiusX.HasValue) return true;
            if (RadiusLeft.HasValue) return true;
            if (RadiusRight.HasValue) return true;
            if (RadiusY.HasValue) return true;
            if (RadiusTop.HasValue) return true;
            if (RadiusBottom.HasValue) return true;
            
            return false;
        }

        public decimal? GetCurveTop() => GetCurveValue(CurveTop, CurveY);
        public decimal? GetCurveBottom() => GetCurveValue(CurveBottom, CurveY);
        public decimal? GetCurveLeft() => GetCurveValue(CurveLeft, CurveX);
        public decimal? GetCurveRight() => GetCurveValue(CurveRight, CurveX);

        public decimal? GetRadiusTop() => RadiusTop ?? RadiusY;
        public decimal? GetRadiusBottom() => RadiusBottom ?? RadiusY;
        public decimal? GetRadiusLeft() => RadiusLeft ?? RadiusX;
        public decimal? GetRadiusRight() => RadiusRight ?? RadiusX;

        private static decimal? GetCurveValue(in decimal? sideValue, in decimal? axisValue)
        {
            if (sideValue.HasValue) return sideValue.Value;
            if (axisValue.HasValue) return axisValue.Value / 2;
            return null;
        }

        #region Deprecated

        [Obsolete("Rename usages of Filter to Method.")]
        public string Filter {
            get => null;
            set => Method = value;
        }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? Noise { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveX { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveLeft { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveRight { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveY { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveTop { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? CurveBottom { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? RadiusX { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? RadiusLeft { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? RadiusRight { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? RadiusY { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? RadiusTop { get; set; }

        [Obsolete("Replace usages with Material-Filters.")]
        public decimal? RadiusBottom { get; set; }

        #endregion
    }
}
