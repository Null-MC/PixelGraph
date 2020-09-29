using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureMap
    {
        public AlbedoTextureMap Albedo {get; set;}
        public HeightTextureMap Height {get; set;}
        public NormalTextureMap Normal {get; set;}
        public SpecularTextureMap Specular {get; set;}


        public TextureMap()
        {
            Albedo = new AlbedoTextureMap();
            Height = new HeightTextureMap();
            Normal = new NormalTextureMap();
            Specular = new SpecularTextureMap();
        }
    }

    internal class AlbedoTextureMap
    {
        public string Texture {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }

    internal class HeightTextureMap
    {
        public string Texture {get; set;}
        public float? Depth {get; set;}
        public bool? NormalizeDepth {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }

    internal class NormalTextureMap
    {
        public string Texture {get; set;}
        public string Heightmap {get; set;}

        [JsonProperty("from-height")]
        public bool? FromHeight {get; set;}

        public int? DownSample {get; set;}
        public float? Strength {get; set;}
        public float? Blur {get; set;}
        public bool? Wrap {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }

    internal class SpecularTextureMap
    {
        public string Texture {get; set;}
        public string Color {get; set;}

        [JsonProperty("metal-min")]
        public float? MetalMin {get; set;}

        [JsonProperty("metal-max")]
        public float? MetalMax {get; set;}

        [JsonProperty("rough-min")]
        public float? RoughMin {get; set;}

        [JsonProperty("rough-max")]
        public float? RoughMax {get; set;}

        [JsonProperty("smooth-min")]
        public float? SmoothMin {get; set;}

        [JsonProperty("smooth-max")]
        public float? SmoothMax {get; set;}

        [JsonProperty("emissive-min")]
        public float? EmissiveMin {get; set;}

        [JsonProperty("emissive-max")]
        public float? EmissiveMax {get; set;}

        [JsonProperty("metal-scale")]
        public float? MetalScale {get; set;}

        [JsonProperty("rough-scale")]
        public float? RoughScale {get; set;}

        [JsonProperty("smooth-scale")]
        public float? SmoothScale {get; set;}

        [JsonProperty("emissive-scale")]
        public float? EmissiveScale {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}

        public bool HasOffsets()
        {
            if (MetalMin.HasValue || MetalMax.HasValue) return true;
            if (RoughMin.HasValue || RoughMax.HasValue) return true;
            if (SmoothMin.HasValue || SmoothMax.HasValue) return true;
            if (EmissiveMin.HasValue || EmissiveMax.HasValue) return true;
            return false;
        }

        public bool HasScaling()
        {
            if (MetalScale.HasValue) return true;
            if (RoughScale.HasValue) return true;
            if (SmoothScale.HasValue) return true;
            if (EmissiveScale.HasValue) return true;
            return false;
        }
    }
}
