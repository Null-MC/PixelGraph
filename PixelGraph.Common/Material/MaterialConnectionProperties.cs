using System;

namespace PixelGraph.Common.Material
{
    public class MaterialConnectionProperties
    {
        public string Method {get; set;}
        public int? Width {get; set;}
        public int? Height {get; set;}
        //public string Tiles {get; set;}
        //public string Weights {get; set;}
        public string MatchBlocks {get; set;}
        public string MatchTiles {get; set;}
        //public string Symmetry {get; set;}
        //public bool? InnerSeams {get; set;}
        //public bool? Linked {get; set;}
        public bool? Placeholder {get; set;}
        public int? TileStartIndex {get; set;}


        #region Deprecated

        [Obsolete("Replace usages of Type with Method")]
        public string Type {
            get => null;
            set => Method = value;
        }

        [Obsolete("Replace usages of CountX with Width")]
        public int? CountX {
            get => null;
            set => Width = value;
        }

        [Obsolete("Replace usages of CountY with Height")]
        public int? CountY {
            get => null;
            set => Height = value;
        }

        #endregion
    }
}
