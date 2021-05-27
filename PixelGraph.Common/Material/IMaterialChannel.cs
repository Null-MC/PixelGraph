namespace PixelGraph.Common.Material
{
    public interface IMaterialChannel
    {
        string Texture {get; set;}
        decimal? Value {get; set;}
        decimal? Scale {get; set;}
    }
}
