using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.TextureFormats.Java;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class JavaTextureFormatValues : List<TextureFormatValueItem>
    {
        public JavaTextureFormatValues()
        {
            Add(new TextureFormatValueItem {Text = "Raw", Value = TextureEncoding.Format_Raw, Hint = RawFormat.Description});
            Add(new TextureFormatValueItem {Text = "Diffuse", Value = TextureEncoding.Format_Diffuse, Hint = DiffuseFormat.Description});
            Add(new TextureFormatValueItem {Text = "Specular", Value = TextureEncoding.Format_Specular, Hint = SpecularFormat.Description});
            Add(new TextureFormatValueItem {Text = "OldPbr", Value = TextureEncoding.Format_OldPbr, Hint = OldPbrFormat.Description});
            Add(new TextureFormatValueItem {Text = "LabPbr 1.1", Value = TextureEncoding.Format_Lab11, Hint = LabPbr11Format.Description});
            Add(new TextureFormatValueItem {Text = "LabPbr 1.2", Value = TextureEncoding.Format_Lab12, Hint = LabPbr12Format.Description});
            Add(new TextureFormatValueItem {Text = "LabPbr 1.3", Value = TextureEncoding.Format_Lab13, Hint = LabPbr13Format.Description});
        }
    }
}
