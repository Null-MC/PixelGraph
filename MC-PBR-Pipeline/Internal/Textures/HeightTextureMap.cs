using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McPbrPipeline.Internal.Textures
{
    internal class HeightTextureMap
    {
        public string Texture {get; set;}
        //public float? DepthScale {get; set;}
        //public bool? NormalizeDepth {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }
}
