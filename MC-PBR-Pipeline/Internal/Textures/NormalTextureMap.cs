using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McPbrPipeline.Internal.Textures
{
    internal class NormalTextureMap
    {
        public string Texture {get; set;}
        public string Heightmap {get; set;}

        public float[] Angle {get; set;}

        [JsonProperty("from-height")]
        public bool? FromHeight {get; set;}

        [JsonProperty("depth-scale")]
        public float? DepthScale {get; set;}

        public int? DownSample {get; set;}
        public float? Strength {get; set;}
        public float? Blur {get; set;}
        public bool? Wrap {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }
}
