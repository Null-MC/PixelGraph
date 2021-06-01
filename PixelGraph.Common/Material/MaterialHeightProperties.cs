using PixelGraph.Common.ResourcePack;
using System;

namespace PixelGraph.Common.Material
{
    public class MaterialHeightProperties
    {
        public ResourcePackHeightChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Shift {get; set;}
        public decimal? Scale {get; set;}
        public decimal? EdgeFadeX {get; set;}
        public decimal? EdgeFadeY {get; set;}


        public bool HasAnyData()
        {
            if (Input != null && Input.HasAnyData()) return true;
            if (Texture != null) return true;
            if (Value.HasValue) return true;
            if (Shift.HasValue) return true;
            if (Scale.HasValue) return true;
            if (EdgeFadeX.HasValue) return true;
            if (EdgeFadeY.HasValue) return true;
            return false;
        }

        #region Deprecated

        [Obsolete] public int? EdgeFadeSizeX {
            get => null;
            set {}
        }

        [Obsolete] public int? EdgeFadeSizeY {
            get => null;
            set {}
        }

        #endregion
    }
}
