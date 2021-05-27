namespace PixelGraph.UI.ViewData
{
    internal class OptionalAllTextureFormatValues : AllTextureFormatValues
    {
        public OptionalAllTextureFormatValues()
        {
            Insert(0, new TextureFormatValueItem {Text = "None", Value = null});
        }
    }
}
