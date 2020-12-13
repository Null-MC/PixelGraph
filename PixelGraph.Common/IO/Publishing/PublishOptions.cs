namespace PixelGraph.Common.IO.Publishing
{
    internal class PublishOptions
    {
        public string Profile {get; set;} = "pack.json";
        public string Destination {get; set;}
        public bool Clean {get; set;} = false;
        public bool Compress {get; set;} = false;
        public string[] Properties {get; set;}
    }
}
