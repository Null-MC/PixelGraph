using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.TextureFormats;
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
        public const string DefaultInputFormat = TextureFormat.Format_Raw;

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

        [YamlMember(Order = -100)]
        public string InputFormat {get; set;}

        [YamlMember(Order = -99)]
        public string Type {get; set;}

        [YamlMember(Order = -98)]
        public bool? Publish {get; set;}

        [YamlMember(Order = -97)]
        public bool? PublishInventory {get; set;}

        [YamlMember(Order = -96)]
        public int? TextureSize {get; set;}

        [YamlMember(Order = -95)]
        public int? TextureWidth {get; set;}

        [YamlMember(Order = -94)]
        public int? TextureHeight {get; set;}

        [YamlMember(Order = -93)]
        public bool? WrapX {get; set;}

        [YamlMember(Order = -92)]
        public bool? WrapY {get; set;}

        //public bool? Resizable {get; set;}

        public int? RangeMin {get; set;}

        public int? RangeMax {get; set;}

        public MaterialAlphaProperties Alpha {get; set;}

        public MaterialAlbedoProperties Albedo {get; set;}

        public MaterialDiffuseProperties Diffuse {get; set;}

        public MaterialHeightProperties Height {get; set;}

        public MaterialBumpProperties Bump {get; set;}

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

        [YamlMember(Alias = "ctm", Order = 100)]
        public MaterialConnectionProperties CTM {get; set;}

        [YamlMember(Order = 101)]
        public List<MaterialPart> Parts {get; set;}

        [YamlMember(Order = 102)]
        public List<MaterialFilter> Filters {get; set;}


        public MaterialProperties()
        {
            Alpha = new MaterialAlphaProperties();
            Albedo = new MaterialAlbedoProperties();
            Diffuse = new MaterialDiffuseProperties();
            Height = new MaterialHeightProperties();
            Bump = new MaterialBumpProperties();
            Normal = new MaterialNormalProperties();
            Occlusion = new MaterialOcclusionProperties();
            Specular = new MaterialSpecularProperties();
            Smooth = new MaterialSmoothProperties();
            Rough = new MaterialRoughProperties();
            Metal = new MaterialMetalProperties();
            F0 = new MaterialF0Properties();
            Porosity = new MaterialPorosityProperties();
            SSS = new MaterialSssProperties();
            Emissive = new MaterialEmissiveProperties();
        }

        public Size GetMultiPartBounds()
        {
            var size = new Size();

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

            return size;
        }

        public bool TryGetSourceBounds(in int? blockSize, in float? scale, out Size size)
        {
            var texWidth = TextureWidth ?? TextureSize;
            var texHeight = TextureHeight ?? TextureSize;
            if (texWidth.HasValue && texHeight.HasValue) {
                size = new Size(texWidth.Value, texHeight.Value);
                if (scale.HasValue) {
                    size.Width = (int)MathF.Ceiling(size.Width * scale.Value);
                    size.Height = (int)MathF.Ceiling(size.Height * scale.Value);
                }
                return true;
            }

            if (!string.IsNullOrWhiteSpace(CTM?.Type)) {
                if (blockSize.HasValue) {
                    switch (CTM.Type) {
                        case CtmTypes.Compact:
                            size = new Size(blockSize.Value * 5, blockSize.Value);
                            return true;
                        case CtmTypes.Full:
                        case CtmTypes.Expanded:
                            size = new Size(blockSize.Value * 12, blockSize.Value * 4);
                            return true;
                        case CtmTypes.Repeat:
                            var countX = CTM.CountX ?? 1;
                            var countY = CTM.CountY ?? 1;
                            size = new Size(blockSize.Value * countX, blockSize.Value * countY);
                            return true;
                    }
                }
            }

            if (Parts?.Any() ?? false) {
                size = GetMultiPartBounds();

                if (scale.HasValue) {
                    size.Width = (int)MathF.Ceiling(size.Width * scale.Value);
                    size.Height = (int)MathF.Ceiling(size.Height * scale.Value);
                }

                return true;
            }

            size = Size.Empty;
            return false;
        }

        public MaterialProperties Clone()
        {
            var clone = (MaterialProperties)MemberwiseClone();

            // TODO: clone child data

            return clone;
        }

        public bool TryGetChannelValue(string encodingChannel, out decimal value)
        {
            if (encodingChannel == null) throw new ArgumentNullException(nameof(encodingChannel));

            decimal? result = null;

            if (decimal.TryParse(encodingChannel, out var valueT)) {
                value = valueT;
                return true;
            }

            if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                result = valueFunc(this);
                value = result ?? 0m;
            }
            else value = 0;

            return result.HasValue;
        }

        public decimal GetChannelScale(string encodingChannel)
        {
            if (EncodingChannel.IsEmpty(encodingChannel)) return 1m;
            return scaleMap.TryGetValue(encodingChannel, out var valueFunc) ? valueFunc(this) : 1m;
        }

        public decimal GetChannelShift(string encodingChannel)
        {
            if (EncodingChannel.IsEmpty(encodingChannel)) return 0m;
            return shiftMap.TryGetValue(encodingChannel, out var valueFunc) ? valueFunc(this) : 0m;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, decimal?>> valueMap = new(StringComparer.OrdinalIgnoreCase) {
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

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> shiftMap = new(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Alpha] = mat => mat.Alpha?.Shift ?? 0m,
            [EncodingChannel.DiffuseRed] = mat => 0m,
            [EncodingChannel.DiffuseGreen] = mat => 0m,
            [EncodingChannel.DiffuseBlue] = mat => 0m,
            [EncodingChannel.AlbedoRed] = mat => 0m,
            [EncodingChannel.AlbedoGreen] = mat => 0m,
            [EncodingChannel.AlbedoBlue] = mat => 0m,
            [EncodingChannel.Height] = mat => mat.Height?.Shift ?? 0m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Shift ?? 0m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Shift ?? 0m,
            [EncodingChannel.Rough] = mat => 0m,
            [EncodingChannel.Metal] = mat => 0m,
            [EncodingChannel.F0] = mat => 0m,
            [EncodingChannel.Porosity] = mat => 0m,
            [EncodingChannel.SubSurfaceScattering] = mat => 0m,
            [EncodingChannel.Emissive] = mat => 0m,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> scaleMap = new(StringComparer.OrdinalIgnoreCase) {
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

        #region Deprecated

        [Obsolete("Replace usages of Wrap with WrapX and WrapY")]
        public bool? Wrap {
            get => null;
            set {
                WrapX = value;
                WrapY = value;
            }
        }
        
        [Obsolete("Replace usages of CreateInventory with PublishInventory")]
        [YamlMember] public bool? CreateInventory {
            get => null;
            set => PublishInventory = value;
        }

        [Obsolete("Replace usages of CtmType with CTM.Type")]
        public string CtmType {
            get => null;
            set {
                CTM ??= new MaterialConnectionProperties();
                CTM.Type = value;
            }
        }

        [Obsolete("Replace usages of CtmCountX with CTM.CountX")]
        public int? CtmCountX {
            get => null;
            set {
                CTM ??= new MaterialConnectionProperties();
                CTM.CountX = value;
            }
        }

        [Obsolete("Replace usages of CtmCountY with CTM.CountY")]
        public int? CtmCountY {
            get => null;
            set {
                CTM ??= new MaterialConnectionProperties();
                CTM.CountY = value;
            }
        }

        #endregion
    }
}
