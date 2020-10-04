namespace McPbrPipeline.Internal.Publishing
{
    internal class PublishOptions
    {
        public string Profile {get; set;} = "pack.json";
        public string Destination {get; set;}
        public bool Clean {get; set;} = false;
        public bool Compress {get; set;} = false;
    }
}
