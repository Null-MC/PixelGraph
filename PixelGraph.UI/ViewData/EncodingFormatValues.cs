using PixelGraph.Common.Encoding;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class EncodingFormatValues : List<EncodingFormatValues.Item>
    {
        public EncodingFormatValues()
        {
            Add(new Item {Text = "Raw", Value = TextureEncoding.Format_Raw, Hint = "All texture channel mappings."});
            Add(new Item {Text = "Albedo", Value = TextureEncoding.Format_Albedo, Hint = "Only Albedo colors and Alpha."});
            Add(new Item {Text = "Diffuse", Value = TextureEncoding.Format_Diffuse, Hint = "Only Diffuse colors and Alpha."});
            Add(new Item {Text = "Specular", Value = TextureEncoding.Format_Specular, Hint = "Greyscale specular."});
            Add(new Item {Text = "Legacy", Value = TextureEncoding.Format_Legacy, Hint = "Legacy PBR encoding."});
            Add(new Item {Text = "LAB 1.1", Value = TextureEncoding.Format_Lab11, Hint = "LabPBR 1.1 encoding."});
            Add(new Item {Text = "LAB 1.2", Value = TextureEncoding.Format_Lab12, Hint = "LabPBR 1.2 encoding."});
            Add(new Item {Text = "LAB 1.3", Value = TextureEncoding.Format_Lab13, Hint = "LabPBR 1.3 encoding."});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
            public string Hint {get; set;}
        }
    }
}
