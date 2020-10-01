using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McPbrPipeline.Internal.Textures
{
    internal class AlbedoTextureMap
    {
        public string Texture {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }
}
