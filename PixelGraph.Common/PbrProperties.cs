using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common
{
    public class PbrProperties : PropertiesFile
    {

        public string FileName {get; set;}
        public string Name {get; set;}
        public string Alias {get; set;}
        public string Path {get; set;}
        public bool UseGlobalMatching {get; set;}

        public string DisplayName => Alias != null ? $"{Alias}:{Name}" : Name;

        public const string Key_Wrap = "wrap";

        public bool Wrap {
            get => Get(Key_Wrap, true);
            set {
                Set(Key_Wrap, value);
                OnPropertyChanged();
            }
        }

        public bool ResizeEnabled => Get("resize.enabled", true);

        public string InputFormat {
            get => Get<string>("input.format");
            set {
                Set("input.format", value);
                OnPropertyChanged();
            }
        }

        public int? RangeMin => Get<int?>("range.min");
        public int? RangeMax => Get<int?>("range.max");

        public const string Key_AlbedoTexture = "albedo.texture";

        public string AlbedoTexture {
            get => Get<string>(Key_AlbedoTexture);
            set {
                Set(Key_AlbedoTexture, value);
                OnPropertyChanged();
            }
        }

        public byte? AlbedoValueR => Get<byte?>("albedo.value.r");
        public byte? AlbedoValueG => Get<byte?>("albedo.value.g");
        public byte? AlbedoValueB => Get<byte?>("albedo.value.b");
        public byte? AlbedoValueA => Get<byte?>("albedo.value.a");
        public float? AlbedoScaleR => Get<float?>("albedo.scale.r");
        public float? AlbedoScaleG => Get<float?>("albedo.scale.g");
        public float? AlbedoScaleB => Get<float?>("albedo.scale.b");
        public float? AlbedoScaleA => Get<float?>("albedo.scale.a");

        public const string Key_HeightTexture = "height.texture";

        public string HeightTexture {
            get => Get<string>(Key_HeightTexture);
            set {
                Set(Key_HeightTexture, value);
                OnPropertyChanged();
            }
        }

        public const string Key_HeightValue = "height.value";

        public byte? HeightValue {
            get => Get<byte?>(Key_HeightValue);
            set {
                Set(Key_HeightValue, value);
                OnPropertyChanged();
            }
        }

        public const string Key_HeightScale = "height.scale";

        public float? HeightScale {
            get => Get<float?>(Key_HeightScale);
            set {
                Set(Key_HeightScale, value);
                OnPropertyChanged();
            }
        }

        public const string Key_NormalTexture = "normal.texture";

        public string NormalTexture {
            get => Get<string>(Key_NormalTexture);
            set {
                Set(Key_NormalTexture, value);
                OnPropertyChanged();
            }
        }

        public byte? NormalValueX => Get<byte?>("normal.value.x");
        public byte? NormalValueY => Get<byte?>("normal.value.y");
        public byte? NormalValueZ => Get<byte?>("normal.value.z");

        public const string Key_NormalStrength = "normal.strength";

        public float? NormalStrength {
            get => Get<float?>(Key_NormalStrength);
            set {
                Set(Key_NormalStrength, value);
                OnPropertyChanged();
            }
        }

        public const string Key_NormalNoise = "normal.noise";

        public float? NormalNoise {
            get => Get<float?>(Key_NormalNoise);
            set {
                Set(Key_NormalNoise, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionTexture = "occlusion.texture";

        public string OcclusionTexture {
            get => Get<string>(Key_OcclusionTexture);
            set {
                Set(Key_OcclusionTexture, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionValue = "occlusion.value";

        public byte? OcclusionValue {
            get => Get<byte?>(Key_OcclusionValue);
            set {
                Set(Key_OcclusionValue, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionScale = "occlusion.scale";

        public float? OcclusionScale {
            get => Get<float?>(Key_OcclusionScale);
            set {
                Set(Key_OcclusionScale, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionZScale = "occlusion.z-scale";
        public const float Default_OcclusionZScale = 25f;

        public float? OcclusionZScale {
            get => Get<float?>(Key_OcclusionZScale);
            set {
                Set(Key_OcclusionZScale, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionZBias = "occlusion.z-bias";
        public const float Default_OcclusionZBias = 1.0f;

        public float? OcclusionZBias {
            get => Get<float?>(Key_OcclusionZBias);
            set {
                Set(Key_OcclusionZBias, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionQuality = "occlusion.quality";
        public const float Default_OcclusionQuality = 0.1f;

        public float? OcclusionQuality {
            get => Get<float?>(Key_OcclusionQuality);
            set {
                Set(Key_OcclusionQuality, value);
                OnPropertyChanged();
            }
        }

        public const string Key_OcclusionSteps = "occlusion.steps";
        public const int Default_OcclusionSteps = 32;

        public int? OcclusionSteps {
            get => Get<int?>(Key_OcclusionSteps);
            set {
                Set(Key_OcclusionSteps, value);
                OnPropertyChanged();
            }
        }

        public bool? OcclusionClipEmissive => Get<bool?>("occlusion.clip-emissive");

        public const string Key_SpecularTexture = "specular.texture";

        public string SpecularTexture {
            get => Get<string>(Key_SpecularTexture);
            set {
                Set(Key_SpecularTexture, value);
                OnPropertyChanged();
            }
        }

        public const string Key_SmoothTexture = "smooth.texture";

        public string SmoothTexture {
            get => Get<string>(Key_SmoothTexture);
            set {
                Set(Key_SmoothTexture, value);
                OnPropertyChanged();
            }
        }

        public const string Key_SmoothValue = "smooth.value";

        public byte? SmoothValue {
            get => Get<byte?>(Key_SmoothValue);
            set {
                Set(Key_SmoothValue, value);
                OnPropertyChanged();
            }
        }

        public const string Key_SmoothScale = "smooth.scale";

        public float? SmoothScale {
            get => Get<float?>(Key_SmoothScale);
            set {
                Set(Key_SmoothScale, value);
                OnPropertyChanged();
            }
        }

        public const string Key_RoughTexture = "rough.texture";

        public string RoughTexture {
            get => Get<string>(Key_RoughTexture);
            set {
                Set(Key_RoughTexture, value);
                OnPropertyChanged();
            }
        }

        public const string Key_RoughValue = "rough.value";

        public byte? RoughValue {
            get => Get<byte?>(Key_RoughValue);
            set {
                Set(Key_RoughValue, value);
                OnPropertyChanged();
            }
        }

        public const string Key_RoughScale = "rough.scale";

        public float? RoughScale {
            get => Get<float?>(Key_RoughScale);
            set {
                Set(Key_RoughScale, value);
                OnPropertyChanged();
            }
        }

        public string MetalTexture => Get<string>("metal.texture");
        public byte? MetalValue => Get<byte?>("metal.value");
        public float? MetalScale => Get<float?>("metal.scale");

        public string PorosityTexture => Get<string>("porosity.texture");
        public byte? PorosityValue => Get<byte?>("porosity.value");
        public float? PorosityScale => Get<float?>("porosity.scale");

        public string SubSurfaceScatteringTexture => Get<string>("sss.texture");
        public byte? SubSurfaceScatteringValue => Get<byte?>("sss.value");
        public float? SubSurfaceScatteringScale => Get<float?>("sss.scale");

        public string EmissiveTexture => Get<string>("emissive.texture");
        public byte? EmissiveValue => Get<byte?>("emissive.value");
        public float? EmissiveScale => Get<float?>("emissive.scale");


        public IEnumerable<string> GetAllTextures(IInputReader reader)
        {
            return TextureTags.All
                .SelectMany(tag => reader.EnumerateTextures(this, tag))
                .Where(file => file != null).Distinct();
        }

        public PbrProperties Clone()
        {
            return new PbrProperties {
                FileName = FileName,
                Name = Name,
                Path = Path,
                UseGlobalMatching = UseGlobalMatching,
                Properties = new Dictionary<string, string>(Properties, StringComparer.InvariantCultureIgnoreCase),
            };
        }
    }
}
