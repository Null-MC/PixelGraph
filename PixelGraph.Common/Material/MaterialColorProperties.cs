using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using System;
using System.Globalization;

namespace PixelGraph.Common.Material
{
    public class MaterialColorProperties : IHaveData
    {
        public const bool DefaultBakeOcclusion = false;

        public string Texture {get; set;}
        public string Value {get; set;}
        public bool? BakeOcclusion {get; set;}


        public ResourcePackColorRedChannelProperties InputRed {get; set;}
        public decimal? ScaleRed {get; set;}

        public ResourcePackColorGreenChannelProperties InputGreen {get; set;}
        public decimal? ScaleGreen {get; set;}

        public ResourcePackColorBlueChannelProperties InputBlue {get; set;}
        public decimal? ScaleBlue {get; set;}


        public byte? GetValueRed()
        {
            var trimValue = Value?.Trim('#', ' ');
            if (string.IsNullOrEmpty(trimValue) || trimValue.Length != 6) return null;
            return byte.Parse(trimValue[0..2], NumberStyles.HexNumber);
        }

        public byte? GetValueGreen()
        {
            var trimValue = Value?.Trim('#', ' ');
            if (string.IsNullOrEmpty(trimValue) || trimValue.Length != 6) return null;
            return byte.Parse(trimValue[2..4], NumberStyles.HexNumber);
        }

        public byte? GetValueBlue()
        {
            var trimValue = Value?.Trim('#', ' ');
            if (string.IsNullOrEmpty(trimValue) || trimValue.Length != 6) return null;
            return byte.Parse(trimValue[4..6], NumberStyles.HexNumber);
        }

        public bool HasAnyData()
        {
            if (Texture != null) return true;
            if (Value != null) return true;
            if (BakeOcclusion.HasValue) return true;

            if (InputRed != null && InputRed.HasAnyData()) return true;
            if (InputGreen != null && InputGreen.HasAnyData()) return true;
            if (InputBlue != null && InputBlue.HasAnyData()) return true;

            if (ScaleRed.HasValue) return true;
            if (ScaleGreen.HasValue) return true;
            if (ScaleBlue.HasValue) return true;
            
            return false;
        }

        #region Deprecated

        internal decimal? __ValueRed;
        [Obsolete("Replace usages of ValueRed with Value")]
        public decimal? ValueRed {
            get => null;
            set => __ValueRed = value;
        }

        internal decimal? __ValueGreen;
        [Obsolete("Replace usages of ValueGreen with Value")]
        public decimal? ValueGreen {
            get => null;
            set => __ValueGreen = value;
        }

        internal decimal? __ValueBlue;
        [Obsolete("Replace usages of ValueBlue with Value")]
        public decimal? ValueBlue {
            get => null;
            set => __ValueBlue = value;
        }

        internal string __PreviewTint;
        [Obsolete("Replace usages of mat.Color.PreviewTint with mat.ColorTint")]
        public string PreviewTint {
            get => null;
            set => __PreviewTint = value;
        }

        #endregion
    }
}
