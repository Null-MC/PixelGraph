using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
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
        public string LocalFilename {get; set;}

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

        public MaterialAlphaProperties Alpha {get; set;}

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


        public ResourcePackChannelProperties GetChannelEncoding(string channel)
        {
            return inputChannelMap.TryGetValue(channel, out var encoding) ? encoding(this) : null;
        }

        public MaterialProperties Clone()
        {
            var clone = (MaterialProperties)MemberwiseClone();

            // TODO: clone child data

            return clone;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, ResourcePackChannelProperties>> inputChannelMap
            = new Dictionary<string, Func<MaterialProperties, ResourcePackChannelProperties>>(StringComparer.InvariantCultureIgnoreCase) {
                [EncodingChannel.Alpha] = m => m.Alpha?.Input,
                [EncodingChannel.DiffuseRed] = m => m.Diffuse?.InputRed,
                [EncodingChannel.DiffuseGreen] = m => m.Diffuse?.InputGreen,
                [EncodingChannel.DiffuseBlue] = m => m.Diffuse?.InputBlue,
                [EncodingChannel.AlbedoRed] = m => m.Albedo?.InputRed,
                [EncodingChannel.AlbedoGreen] = m => m.Albedo?.InputGreen,
                [EncodingChannel.AlbedoBlue] = m => m.Albedo?.InputBlue,
                [EncodingChannel.Height] = m => m.Height?.Input,
                [EncodingChannel.Occlusion] = m => m.Occlusion?.Input,
                [EncodingChannel.NormalX] = m => m.Normal?.InputX,
                [EncodingChannel.NormalY] = m => m.Normal?.InputY,
                [EncodingChannel.NormalZ] = m => m.Normal?.InputZ,
                [EncodingChannel.Specular] = m => m.Specular?.Input,
                [EncodingChannel.Smooth] = m => m.Smooth?.Input,
                [EncodingChannel.Rough] = m => m.Rough?.Input,
                [EncodingChannel.Metal] = m => m.Metal?.Input,
                [EncodingChannel.Porosity] = m => m.Porosity?.Input,
                [EncodingChannel.SubSurfaceScattering] = m => m.SSS?.Input,
                [EncodingChannel.Emissive] = m => m.Emissive?.Input,
            };
    }
}
