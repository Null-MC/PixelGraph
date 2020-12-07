using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialProperties
    {
        public const bool DefaultWrap = true;
        //public const bool DefaultResizeEnabled = true;
        public const string DefaultInputFormat = TextureEncoding.Format_Raw;

        [YamlIgnore]
        public string Name {get; set;}

        [YamlIgnore]
        public bool UseGlobalMatching {get; set;}

        [YamlIgnore]
        public string LocalFilename {get; internal set;}

        [YamlIgnore]
        public string LocalPath {get; set;}

        [YamlIgnore]
        public string Alias {get; internal set;}

        [YamlIgnore]
        public string DisplayName => Alias != null ? $"{Alias}:{Name}" : Name;

        [YamlIgnore]
        public bool IsMultiPart => Parts?.Any() ?? false;

        [YamlMember(Order = 0)]
        public string InputFormat {get; set;}

        public bool? Wrap {get; set;}

        public bool? ResizeEnabled {get; set;}

        public int? RangeMin {get; set;}

        public int? RangeMax {get; set;}

        public MaterialAlbedoProperties Albedo {get; set;}

        public MaterialDiffuseProperties Diffuse {get; set;}

        public MaterialHeightProperties Height {get; set;}

        public MaterialNormalProperties Normal {get; set;}

        public MaterialOcclusionProperties Occlusion {get; set;}

        public MaterialSpecularProperties Specular {get; set;}

        public MaterialSmoothProperties Smooth {get; set;}

        public MaterialRoughProperties Rough {get; set;}

        public MaterialMetalProperties Metal {get; set;}

        public MaterialPorosityProperties Porosity {get; set;}

        [YamlMember(Alias = "sss", ApplyNamingConventions = false)]
        public MaterialSssProperties SSS {get; set;}

        public MaterialEmissiveProperties Emissive {get; set;}

        [YamlMember(Order = 99)]
        public List<MaterialPart> Parts {get; set;}


        //public MaterialProperties()
        //{
        //    //Regions = new List<MaterialRegion>();

        //    //Albedo = new MaterialAlbedoProperties();
        //    //Height = new MaterialHeightProperties();
        //    //Normal = new MaterialNormalProperties();
        //    //Occlusion = new MaterialOcclusionProperties();
        //    //Specular = new MaterialSpecularProperties();
        //    //Smooth = new MaterialSmoothProperties();
        //    //Rough = new MaterialRoughProperties();
        //    //Metal = new MaterialMetalProperties();
        //    //Porosity = new MaterialPorosityProperties();
        //    //SSS = new MaterialSssProperties();
        //    //Emissive = new MaterialEmissiveProperties();
        //}

        public TextureEncoding GetInputEncoding(string tag)
        {
            return textureInputMap.TryGetValue(tag, out var encoding) ? encoding(this) : null;
        }

        public MaterialProperties Clone()
        {
            var clone = (MaterialProperties)MemberwiseClone();

            // TODO: clone child data

            return clone;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, TextureEncoding>> textureInputMap
            = new Dictionary<string, Func<MaterialProperties, TextureEncoding>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = m => m.Albedo?.Input,
                [TextureTags.Height] = m => m.Height?.Input,
                [TextureTags.Normal] = m => m.Normal?.Input,
                [TextureTags.Occlusion] = m => m.Occlusion?.Input,
                [TextureTags.Specular] = m => m.Specular?.Input,
                [TextureTags.Smooth] = m => m.Smooth?.Input,
                [TextureTags.Rough] = m => m.Rough?.Input,
                [TextureTags.Metal] = m => m.Metal?.Input,
                [TextureTags.Porosity] = m => m.Porosity?.Input,
                [TextureTags.SubSurfaceScattering] = m => m.SSS?.Input,
                [TextureTags.Emissive] = m => m.Emissive?.Input,
            };
    }
}
