﻿using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialSmoothProperties
    {
        public ResourcePackSmoothChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Scale {get; set;}
    }
}
