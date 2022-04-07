using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PixelGraph.Common.IO.Texture
{
    internal class TextureSet_v1_16_100
    {
        public JToken Color {get; set;}
        public JToken MER {get; set;}
        public JToken HeightMap {get; set;}
    }
}
