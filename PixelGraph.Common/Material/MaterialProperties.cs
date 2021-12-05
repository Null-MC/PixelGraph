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
        //public const string DefaultModelType = Models.ModelType.Cube;
        public const bool DefaultPublish = true;
        public const bool DefaultPublishItem = false;

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
        public bool? PublishItem {get; set;}

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

        public int? RangeMin {get; set;}

        public int? RangeMax {get; set;}

        public MaterialOpacityProperties Opacity {get; set;}

        public MaterialColorProperties Color {get; set;}

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

        public string Model {get; set;}

        public string BlendMode {get; set;}

        public string TintColor {get; set;}

        [YamlMember(Alias = "ctm", Order = 100)]
        public MaterialConnectionProperties CTM {get; set;}

        [YamlMember(Order = 101)]
        public List<MaterialPart> Parts {get; set;}

        [YamlMember(Order = 102)]
        public List<MaterialFilter> Filters {get; set;}


        public MaterialProperties()
        {
            Opacity = new MaterialOpacityProperties();
            Color = new MaterialColorProperties();
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

            //BlendMode = BlendModes.Opaque;
        }

        public bool TryGetPartIndex(string name, out int partIndex)
        {
            if (Parts == null) {
                partIndex = 0;
                return false;
            }

            for (var i = 0; i < Parts.Count; i++) {
                if (!string.Equals(Parts[i].Name, name, StringComparison.InvariantCultureIgnoreCase)) continue;

                partIndex = i;
                return true;
            }

            //part = Parts.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            //return part != null;
            partIndex = 0;
            return false;
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
            // ERROR: this is fucking things up!
            // TODO: move type-based size lookup to it's own method

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

            if (!string.IsNullOrWhiteSpace(CTM?.Method) && blockSize.HasValue) {
                var bounds = CtmTypes.GetBounds(CTM);

                if (bounds != null) {
                    size = new Size(blockSize.Value * bounds.Width, blockSize.Value * bounds.Height);
                    return true;
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

        public MaterialType GetMaterialType()
        {
            if (string.IsNullOrWhiteSpace(Type))
                return MaterialType.Automatic;

            if (Enum.TryParse(typeof(MaterialType), Type, out var type) && type != null)
                return (MaterialType) type;

            return MaterialType.Automatic;
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

        //public decimal? GetChannelDefaultValue(string encodingChannel)
        //{
        //    if (encodingChannel == null) throw new ArgumentNullException(nameof(encodingChannel));

        //    if (decimal.TryParse(encodingChannel, out var valueT)) return valueT;

        //    return defaultValueMap.TryGetValue(encodingChannel, out var valueFunc) ? valueFunc(this) : null;
        //}

        //public decimal? GetChannelClipValue(string encodingChannel)
        //{
        //    if (encodingChannel == null) throw new ArgumentNullException(nameof(encodingChannel));

        //    if (decimal.TryParse(encodingChannel, out var valueT)) return valueT;

        //    return clipValueMap.TryGetValue(encodingChannel, out var valueFunc) ? valueFunc(this) : null;
        //}

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
            [EncodingChannel.Opacity] = mat => mat.Opacity?.Value,
            [EncodingChannel.ColorRed] = mat => mat.Color?.ValueRed,
            [EncodingChannel.ColorGreen] = mat => mat.Color?.ValueGreen,
            [EncodingChannel.ColorBlue] = mat => mat.Color?.ValueBlue,
            [EncodingChannel.Height] = mat => mat.Height?.Value,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Value,
            [EncodingChannel.NormalX] = mat => mat.Normal?.ValueX,
            [EncodingChannel.NormalY] = mat => mat.Normal?.ValueY,
            [EncodingChannel.NormalZ] = mat => mat.Normal?.ValueZ,
            [EncodingChannel.Specular] = mat => mat.Specular?.Value,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Value,
            [EncodingChannel.Rough] = mat => mat.Rough?.Value,
            [EncodingChannel.Metal] = mat => mat.Metal?.Value,
            [EncodingChannel.F0] = mat => mat.F0?.Value,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Value,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Value,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Value,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> shiftMap = new(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Opacity] = mat => mat.Opacity?.Shift ?? 0m,
            [EncodingChannel.ColorRed] = mat => 0m,
            [EncodingChannel.ColorGreen] = mat => 0m,
            [EncodingChannel.ColorBlue] = mat => 0m,
            [EncodingChannel.Height] = mat => mat.Height?.Shift ?? 0m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Shift ?? 0m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Shift ?? 0m,
            [EncodingChannel.Specular] = mat => mat.Specular?.Shift ?? 0m,
            [EncodingChannel.Rough] = mat => mat.Rough?.Shift ?? 0m,
            [EncodingChannel.Metal] = mat => 0m,
            [EncodingChannel.F0] = mat => mat.F0?.Shift ?? 0m,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Shift ?? 0m,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Shift ?? 0m,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Shift ?? 0m,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> scaleMap = new(StringComparer.OrdinalIgnoreCase) {
            [EncodingChannel.Opacity] = mat => mat.Opacity?.Scale ?? 1m,
            [EncodingChannel.ColorRed] = mat => mat.Color?.ScaleRed ?? 1m,
            [EncodingChannel.ColorGreen] = mat => mat.Color?.ScaleGreen ?? 1m,
            [EncodingChannel.ColorBlue] = mat => mat.Color?.ScaleBlue ?? 1m,
            [EncodingChannel.Height] = mat => mat.Height?.Scale ?? 1m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Scale ?? 1m,
            [EncodingChannel.Specular] = mat => mat.Specular?.Scale ?? 1m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Scale ?? 1m,
            [EncodingChannel.Rough] = mat => mat.Rough?.Scale ?? 1m,
            [EncodingChannel.Metal] = mat => mat.Metal?.Scale ?? 1m,
            [EncodingChannel.F0] = mat => mat.F0?.Scale ?? 1m,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Scale ?? 1m,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Scale ?? 1m,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Scale ?? 1m,
        };

        #region Deprecated

        [Obsolete("Replace usages of PublishInventory with PublishItem")]
        public bool? PublishInventory {
            get => null;
            set => PublishItem = value;
        }

        //[Obsolete("Replace usages of ColorTint with Color.ColorTint")]
        //public string ColorTint {
        //    get => null;
        //    set {
        //        Color ??= new MaterialColorProperties();
        //        Color.PreviewTint = value;
        //    }
        //}

        [Obsolete("Replace usages of BakeOcclusionToColor with Color.BakeOcclusion")]
        public bool? BakeOcclusionToColor {
            get => null;
            set {
                Color ??= new MaterialColorProperties();
                Color.BakeOcclusion = value;
            }
        }

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
            set => PublishItem = value;
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

        [Obsolete("Replace usages of Alpha with Opacity")]
        public MaterialOpacityProperties Alpha {
            get => null;
            set => Opacity = value;
        }

        [Obsolete("Replace usages of Albedo with Color")]
        public MaterialColorProperties Albedo {
            get => null;
            set => Color = value;
        }

        [Obsolete("Replace usages of Diffuse with Color")]
        public MaterialColorProperties Diffuse {
            get => null;
            set => Color = value;
        }

        [Obsolete("Replace usages of ModelType with Model")]
        public string ModelType {
            get => null;
            set {}
            //set => Model = value;
        }

        [Obsolete("Replace usages of ModelFile with Model")]
        public string ModelFile {
            get => null;
            set => Model = value;
        }

        [Obsolete("Replace usages of ColorTint with TintColor")]
        public string ColorTint {
            get => null;
            set => TintColor = value;
        }

        #endregion
    }
}
