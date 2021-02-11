using PixelGraph.Common.Encoding;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material
{
    public class MaterialProperties
    {
        public const bool DefaultWrap = true;
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

        [YamlMember(Order = 1)]
        public string Type {get; set;}

        [YamlMember(Order = 2)]
        public int? TextureSize {get; set;}

        [YamlMember(Order = 3)]
        public int? TextureWidth {get; set;}

        [YamlMember(Order = 4)]
        public int? TextureHeight {get; set;}

        [YamlMember(Order = 5)]
        public bool? WrapX {get; set;}

        [YamlMember(Order = 6)]
        public bool? WrapY {get; set;}

        public bool? Resizable {get; set;}

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

        public MaterialF0Properties F0 {get; set;}

        public MaterialPorosityProperties Porosity {get; set;}

        [YamlMember(Alias = "sss", ApplyNamingConventions = false)]
        public MaterialSssProperties SSS {get; set;}

        public MaterialEmissiveProperties Emissive {get; set;}

        [YamlMember(Order = 99)]
        public List<MaterialPart> Parts {get; set;}


        public bool TryGetSourceBounds(out Size size)
        {
            if (!IsMultiPart) {
                size = Size.Empty;
                return false;
            }

            size = new Size();
            foreach (var part in Parts) {
                if (part.Left.HasValue && part.Width.HasValue) {
                    var partWidth = part.Left.Value + part.Width.Value;
                    if (partWidth > size.Width) size.Width = partWidth;
                }

                if (part.Top.HasValue && part.Height.HasValue) {
                    var partHeight = part.Top.Value + part.Height.Value;
                    if (partHeight > size.Height) size.Height = partHeight;
                }
            }

            return true;
        }

        public MaterialProperties Clone()
        {
            var clone = (MaterialProperties)MemberwiseClone();

            // TODO: clone child data

            return clone;
        }

        public bool TryGetChannelValue(string encodingChannel, out decimal value)
        {
            decimal? result = null;

            if (decimal.TryParse(encodingChannel, out value)) return true;

            if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                result = valueFunc(this);
                value = result ?? 0m;
            }
            else value = 0;

            return result.HasValue;
        }

        public float GetChannelScale(string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 1f;
            return scaleMap.TryGetValue(channel, out var value) ? (float)value(this) : 1f;
        }

        public float GetChannelShift(string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 0f;
            return shiftMap.TryGetValue(channel, out var value) ? (float)value(this) : 0f;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, decimal?>> valueMap = new Dictionary<string, Func<MaterialProperties, decimal?>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Value,
            [EncodingChannel.DiffuseRed] = mat => mat.Diffuse?.ValueRed,
            [EncodingChannel.DiffuseGreen] = mat => mat.Diffuse?.ValueGreen,
            [EncodingChannel.DiffuseBlue] = mat => mat.Diffuse?.ValueBlue,
            [EncodingChannel.AlbedoRed] = mat => mat.Albedo?.ValueRed,
            [EncodingChannel.AlbedoGreen] = mat => mat.Albedo?.ValueGreen,
            [EncodingChannel.AlbedoBlue] = mat => mat.Albedo?.ValueBlue,
            [EncodingChannel.Height] = mat => mat.Height?.Value,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Value,
            [EncodingChannel.NormalX] = mat => mat.Normal?.ValueX,
            [EncodingChannel.NormalY] = mat => mat.Normal?.ValueY,
            [EncodingChannel.NormalZ] = mat => mat.Normal?.ValueZ,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Value,
            [EncodingChannel.Rough] = mat => mat.Rough?.Value,
            [EncodingChannel.Metal] = mat => mat.Metal?.Value,
            [EncodingChannel.F0] = mat => mat.F0?.Value,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Value,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Value,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Value,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> shiftMap = new Dictionary<string, Func<MaterialProperties, decimal>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => 0,
            [EncodingChannel.DiffuseRed] = mat => 0,
            [EncodingChannel.DiffuseGreen] = mat => 0,
            [EncodingChannel.DiffuseBlue] = mat => 0,
            [EncodingChannel.AlbedoRed] = mat => 0,
            [EncodingChannel.AlbedoGreen] = mat => 0,
            [EncodingChannel.AlbedoBlue] = mat => 0,
            [EncodingChannel.Height] = mat => mat.Height?.Shift ?? 0,
            [EncodingChannel.Occlusion] = mat => 0,
            [EncodingChannel.Smooth] = mat => 0,
            [EncodingChannel.Rough] = mat => 0,
            [EncodingChannel.Metal] = mat => 0,
            [EncodingChannel.F0] = mat => 0,
            [EncodingChannel.Porosity] = mat => 0,
            [EncodingChannel.SubSurfaceScattering] = mat => 0,
            [EncodingChannel.Emissive] = mat => 0,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> scaleMap = new Dictionary<string, Func<MaterialProperties, decimal>>(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Scale ?? 1m,
            [EncodingChannel.DiffuseRed] = mat => mat.Diffuse?.ScaleRed ?? 1m,
            [EncodingChannel.DiffuseGreen] = mat => mat.Diffuse?.ScaleGreen ?? 1m,
            [EncodingChannel.DiffuseBlue] = mat => mat.Diffuse?.ScaleBlue ?? 1m,
            [EncodingChannel.AlbedoRed] = mat => mat.Albedo?.ScaleRed ?? 1m,
            [EncodingChannel.AlbedoGreen] = mat => mat.Albedo?.ScaleGreen ?? 1m,
            [EncodingChannel.AlbedoBlue] = mat => mat.Albedo?.ScaleBlue ?? 1m,
            [EncodingChannel.Height] = mat => mat.Height?.Scale ?? 1m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Scale ?? 1m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Scale ?? 1m,
            [EncodingChannel.Rough] = mat => mat.Rough?.Scale ?? 1m,
            [EncodingChannel.Metal] = mat => mat.Metal?.Scale ?? 1m,
            [EncodingChannel.F0] = mat => mat.F0?.Scale ?? 1m,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Scale ?? 1m,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Scale ?? 1m,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Scale ?? 1m,
        };
    }
}
