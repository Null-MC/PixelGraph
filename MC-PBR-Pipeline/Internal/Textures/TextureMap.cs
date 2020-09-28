using Newtonsoft.Json;

namespace McPbrPipeline.Textures
{
    internal class TextureMap
    {
        //[JsonIgnore]
        //public string Name {get; set;}

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
    }

    internal class HeightTextureMap
    {
        public string Texture {get; set;}
        public float Depth {get; set;} = 1f;
        public bool NormalizeDepth {get; set;} = false;
    }

    internal class NormalTextureMap
    {
        public string Texture {get; set;}
        public bool FromHeight {get; set;} = true;
        public bool Wrap {get; set;} = true;
    }

    internal class SpecularTextureMap
    {
        public string Texture {get; set;}
        public string Color {get; set;}
    }
}
