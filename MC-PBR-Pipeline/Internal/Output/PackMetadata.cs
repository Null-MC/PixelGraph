using Newtonsoft.Json;

namespace McPbrPipeline.Internal.Output
{
    internal class PackMetadata
    {
        [JsonProperty("pack_format")]
        public int PackFormat {get; set;}

        [JsonProperty("description")]
        public string Description {get; set;}


        public PackMetadata()
        {
            PackFormat = 5;
        }
    }
}
