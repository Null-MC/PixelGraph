namespace PixelGraph.UI.Internal.IO.Publishing;

public class PublishLocation
{
    public string Name {get; set;}
    public string Path {get; set;}
    public bool Archive {get; set;}
    //public string Clean {get; set;}


    public object Clone()
    {
        var clone = (PublishLocation)MemberwiseClone();
        //...
        return clone;
    }
}