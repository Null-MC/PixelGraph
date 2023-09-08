using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewData;

internal class OptionalTextureTagValues : TextureTagValues
{
    public OptionalTextureTagValues()
    {
        Insert(0, new Item {Text = "None", Value = TextureTags.None});
    }
}