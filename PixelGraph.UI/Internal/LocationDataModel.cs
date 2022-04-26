namespace PixelGraph.UI.Internal
{
    public class LocationDataModel
    {
        public string Name {get; set;}
        public string Path {get; set;}
        public bool Archive {get; set;}
        //public string Clean {get; set;}


        public object Clone()
        {
            var clone = (LocationDataModel)MemberwiseClone();
            //...
            return clone;
        }
    }
}
