using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.TextureFormats.Bedrock;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class BedrockTextureFormatValues : List<TextureFormatValueItem>
    {
        public BedrockTextureFormatValues()
        {
            Add(new TextureFormatValueItem {Text = "Raw", Value = TextureEncoding.Format_Raw, Hint = RawFormat.Description});
            Add(new TextureFormatValueItem {Text = "Diffuse", Value = TextureEncoding.Format_Diffuse, Hint = DiffuseFormat.Description});
            Add(new TextureFormatValueItem {Text = "RTX", Value = TextureEncoding.Format_Rtx, Hint = RtxFormat.Description});
        }
    }
}
