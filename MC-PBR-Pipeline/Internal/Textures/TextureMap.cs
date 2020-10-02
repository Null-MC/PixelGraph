using Newtonsoft.Json;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureMap
    {
        [JsonProperty("disable-resize")]
        public bool? DisableResize {get; set;}

        public AlbedoTextureMap Albedo {get; set;}
        public HeightTextureMap Height {get; set;}
        public NormalTextureMap Normal {get; set;}
        public SpecularTextureMap Specular {get; set;}
        public EmissiveTextureMap Emissive {get; set;}


        public TextureMap()
        {
            Albedo = new AlbedoTextureMap();
            Height = new HeightTextureMap();
            Normal = new NormalTextureMap();
            Specular = new SpecularTextureMap();
            Emissive = new EmissiveTextureMap();
        }
    }
}
