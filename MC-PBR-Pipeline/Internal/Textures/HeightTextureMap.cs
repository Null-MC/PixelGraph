using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McPbrPipeline.Internal.Textures
{
    internal class HeightTextureMap
    {
        public string Texture {get; set;}
        public float? Scale {get; set;}
        //public bool? Normalize {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }
}
