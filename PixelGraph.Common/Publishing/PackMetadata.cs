using Newtonsoft.Json;

namespace PixelGraph.Common.Publishing
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
