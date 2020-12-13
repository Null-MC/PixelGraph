using PixelGraph.Common.Textures;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public abstract class ResourcePackChannelProperties
    {
        [YamlIgnore]
        public string ID {get;}

        public string Texture {get; set;}
        public ColorChannel? Color {get; set;}
        public string Sampler {get; set;}
        public byte? MinValue {get; set;}
        public byte? MaxValue {get; set;}
        public short? Shift {get; set;}
        public decimal? Power {get; set;}
        public bool? Invert {get; set;}

        [YamlIgnore]
        public bool HasMapping => Texture != null && (Color.HasValue && Color.Value != ColorChannel.None);


        protected ResourcePackChannelProperties(string id)
        {
            ID = id;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            // TODO: use StringBuilder and show all options
            return $"{{{ID} {Texture}:{Color}}}";
        }
    }
}
