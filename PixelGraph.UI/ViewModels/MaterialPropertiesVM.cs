using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.UI.ViewData;
using System;
using System.ComponentModel;

namespace PixelGraph.UI.ViewModels
{
    internal class MaterialPropertiesVM : ViewModelBase
    {
        private MaterialProperties _material;
        private string _selectedTag;
        private decimal? _iorToF0Value;
        private decimal? _iorDefaultValue;

        public event EventHandler DataChanged;

        public AlphaProperties Alpha {get;}
        public AlbedoProperties Albedo {get;}
        public DiffuseProperties Diffuse {get;}
        public HeightProperties Height {get;}
        public NormalProperties Normal {get;}
        public NormalGeneratorProperties NormalGeneration {get;}
        public OcclusionProperties Occlusion {get;}
        public OcclusionGeneratorProperties OcclusionGeneration {get;}
        public SpecularProperties Specular {get;}
        public SmoothProperties Smooth {get;}
        public RoughProperties Rough {get;}
        public MetalProperties Metal {get;}
        public F0Properties F0 {get;}
        public PorosityProperties Porosity {get;}
        public SssProperties Sss {get; set;}
        public EmissiveProperties Emissive {get;}

        public bool HasMaterial => _material != null;
        //public bool IsGeneralSelected => _selectedTag == null;
        public bool IsAlphaSelected => TextureTags.Is(_selectedTag, TextureTags.Alpha);
        public bool IsAlbedoSelected => TextureTags.Is(_selectedTag, TextureTags.Albedo);
        public bool IsDiffuseSelected => TextureTags.Is(_selectedTag, TextureTags.Diffuse);
        public bool IsHeightSelected => TextureTags.Is(_selectedTag, TextureTags.Height);
        public bool IsNormalSelected => TextureTags.Is(_selectedTag, TextureTags.Normal);
        public bool IsOcclusionSelected => TextureTags.Is(_selectedTag, TextureTags.Occlusion);
        public bool IsSpecularSelected => TextureTags.Is(_selectedTag, TextureTags.Specular);
        public bool IsSmoothSelected => TextureTags.Is(_selectedTag, TextureTags.Smooth);
        public bool IsRoughSelected => TextureTags.Is(_selectedTag, TextureTags.Rough);
        public bool IsMetalSelected => TextureTags.Is(_selectedTag, TextureTags.Metal);
        public bool IsF0Selected => TextureTags.Is(_selectedTag, TextureTags.F0);
        public bool IsPorositySelected => TextureTags.Is(_selectedTag, TextureTags.Porosity);
        public bool IsSssSelected => TextureTags.Is(_selectedTag, TextureTags.SubSurfaceScattering);
        public bool IsEmissiveSelected => TextureTags.Is(_selectedTag, TextureTags.Emissive);

        public string DefaultInputFormat => MaterialProperties.DefaultInputFormat;

        public decimal? IorActualValue => _iorToF0Value;

        public MaterialProperties Material {
            get => _material;
            set {
                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                UpdateGeneralProperties();
                Alpha.SetData(Material?.Alpha);
                Albedo.SetData(Material?.Albedo);
                Diffuse.SetData(Material?.Diffuse);
                Height.SetData(Material?.Height);
                Normal.SetData(Material?.Normal);
                NormalGeneration.SetData(Material?.Normal);
                Occlusion.SetData(Material?.Occlusion);
                OcclusionGeneration.SetData(Material?.Occlusion);
                Specular.SetData(Material?.Specular);
                Smooth.SetData(Material?.Smooth);
                Rough.SetData(Material?.Rough);
                Metal.SetData(Material?.Metal);
                F0.SetData(Material?.F0);
                Porosity.SetData(Material?.Porosity);
                Sss.SetData(Material?.SSS);
                Emissive.SetData(Material?.Emissive);

                _iorToF0Value = null;
                _iorDefaultValue = CalculateIorFromF0();
                OnPropertyChanged(nameof(IorActualValue));
                OnPropertyChanged(nameof(IorEditValue));
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged();

                //OnPropertyChanged(nameof(IsGeneralSelected));
                OnPropertyChanged(nameof(IsAlphaSelected));
                OnPropertyChanged(nameof(IsAlbedoSelected));
                OnPropertyChanged(nameof(IsDiffuseSelected));
                OnPropertyChanged(nameof(IsHeightSelected));
                OnPropertyChanged(nameof(IsNormalSelected));
                OnPropertyChanged(nameof(IsOcclusionSelected));
                OnPropertyChanged(nameof(IsSpecularSelected));
                OnPropertyChanged(nameof(IsSmoothSelected));
                OnPropertyChanged(nameof(IsRoughSelected));
                OnPropertyChanged(nameof(IsMetalSelected));
                OnPropertyChanged(nameof(IsF0Selected));
                OnPropertyChanged(nameof(IsPorositySelected));
                OnPropertyChanged(nameof(IsSssSelected));
                OnPropertyChanged(nameof(IsEmissiveSelected));
            }
        }

        public decimal? IorEditValue {
            get => IorActualValue ?? _iorDefaultValue;
            set {
                _iorToF0Value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IorActualValue));
            }
        }

        #region General

        public string InputFormat {
            get => _material?.InputFormat;
            set {
                _material.InputFormat = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? WrapX {
            get => _material?.WrapX;
            set {
                _material.WrapX = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? WrapY {
            get => _material?.WrapY;
            set {
                _material.WrapY = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateGeneralProperties()
        {
            OnPropertyChanged(nameof(InputFormat));
            OnPropertyChanged(nameof(WrapX));
            OnPropertyChanged(nameof(WrapY));
        }

        #endregion


        public MaterialPropertiesVM()
        {
            Alpha = new AlphaProperties();
            Alpha.PropertyChanged += OnPropertyValueChanged;

            Albedo = new AlbedoProperties();
            Albedo.PropertyChanged += OnPropertyValueChanged;

            Diffuse = new DiffuseProperties();
            Diffuse.PropertyChanged += OnPropertyValueChanged;

            Height = new HeightProperties();
            Height.PropertyChanged += OnPropertyValueChanged;

            Normal = new NormalProperties();
            Normal.PropertyChanged += OnPropertyValueChanged;

            NormalGeneration = new NormalGeneratorProperties();
            NormalGeneration.PropertyChanged += OnPropertyValueChanged;

            Occlusion = new OcclusionProperties();
            Occlusion.PropertyChanged += OnPropertyValueChanged;

            OcclusionGeneration = new OcclusionGeneratorProperties();
            OcclusionGeneration.PropertyChanged += OnPropertyValueChanged;

            Specular = new SpecularProperties();
            Specular.PropertyChanged += OnPropertyValueChanged;

            Smooth = new SmoothProperties();
            Smooth.PropertyChanged += OnPropertyValueChanged;

            Rough = new RoughProperties();
            Rough.PropertyChanged += OnPropertyValueChanged;

            Metal = new MetalProperties();
            Metal.PropertyChanged += OnPropertyValueChanged;

            F0 = new F0Properties();
            F0.PropertyChanged += OnF0PropertyValueChanged;

            Porosity = new PorosityProperties();
            Porosity.PropertyChanged += OnPropertyValueChanged;

            Sss = new SssProperties();
            Sss.PropertyChanged += OnPropertyValueChanged;

            Emissive = new EmissiveProperties();
            Emissive.PropertyChanged += OnPropertyValueChanged;
        }

        public void ConvertIorToF0()
        {
            if (!_iorToF0Value.HasValue) return;

            var t = (_iorToF0Value - 1) / (_iorToF0Value + 1);
            var f0 = Math.Round((decimal)(t * t), 3);

            Material.F0.Value = f0;
            OnDataChanged();
            F0.Invalidate();

            _iorDefaultValue = _iorToF0Value;
            _iorToF0Value = null;
            OnPropertyChanged(nameof(IorActualValue));
            OnPropertyChanged(nameof(IorEditValue));
        }

        private decimal? CalculateIorFromF0()
        {
            if (!(Material?.F0?.Value.HasValue ?? false)) return null;

            var f0 = (double)Material.F0.Value.Value;
            if (f0 < 0) return null;

            var d = -(f0 + 1 + 2 * Math.Sqrt(f0)) / (f0 - 1);
            return Math.Round((decimal) d, 3);
        }

        private void OnF0PropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName?.Equals(nameof(MaterialF0Properties.Value)) ?? false) {
                _iorToF0Value = null;
                _iorDefaultValue = CalculateIorFromF0();
                OnPropertyChanged(nameof(IorActualValue));
                OnPropertyChanged(nameof(IorEditValue));
            }

            OnDataChanged();
        }

        private void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AlphaProperties : PropertyCollectionBase<MaterialAlphaProperties>
    {
        public AlphaProperties()
        {
            AddText<string>("Texture", nameof(MaterialAlphaProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialAlphaProperties.Value), 255m);
            AddText<decimal?>("Scale", nameof(MaterialAlphaProperties.Scale), 1m);
        }
    }

    public class AlbedoProperties : PropertyCollectionBase<MaterialAlbedoProperties>
    {
        public AlbedoProperties()
        {
            AddText<string>("Texture", nameof(MaterialAlbedoProperties.Texture));
            AddText<decimal?>("Red Value", nameof(MaterialAlbedoProperties.ValueRed), 0);
            AddText<decimal?>("Green Value", nameof(MaterialAlbedoProperties.ValueGreen), 0);
            AddText<decimal?>("Blue Value", nameof(MaterialAlbedoProperties.ValueBlue), 0);
            AddText<decimal?>("Red Scale", nameof(MaterialAlbedoProperties.ScaleRed), 1.0m);
            AddText<decimal?>("Green Scale", nameof(MaterialAlbedoProperties.ScaleGreen), 1.0m);
            AddText<decimal?>("Blue Scale", nameof(MaterialAlbedoProperties.ScaleBlue), 1.0m);
        }
    }

    public class DiffuseProperties : PropertyCollectionBase<MaterialDiffuseProperties>
    {
        public DiffuseProperties()
        {
            AddText<string>("Texture", nameof(MaterialDiffuseProperties.Texture));
            AddText<decimal?>("Red Value", nameof(MaterialDiffuseProperties.ValueRed), 0);
            AddText<decimal?>("Green Value", nameof(MaterialDiffuseProperties.ValueGreen), 0);
            AddText<decimal?>("Blue Value", nameof(MaterialDiffuseProperties.ValueBlue), 0);
            AddText<decimal?>("Red Scale", nameof(MaterialDiffuseProperties.ScaleRed), 1.0m);
            AddText<decimal?>("Green Scale", nameof(MaterialDiffuseProperties.ScaleGreen), 1.0m);
            AddText<decimal?>("Blue Scale", nameof(MaterialDiffuseProperties.ScaleBlue), 1.0m);
        }
    }

    public class HeightProperties : PropertyCollectionBase<MaterialHeightProperties>
    {
        public HeightProperties()
        {
            AddText<string>("Texture", nameof(MaterialHeightProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialHeightProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialHeightProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialHeightProperties.Scale), 1m);
            AddText<decimal?>("Horizontal Edge-Fade", nameof(MaterialHeightProperties.EdgeFadeX));
            AddText<decimal?>("Vertical Edge-Fade", nameof(MaterialHeightProperties.EdgeFadeY));
        }
    }

    public class NormalProperties : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalProperties()
        {
            AddText<string>("Texture", nameof(MaterialNormalProperties.Texture));
        }
    }

    public class NormalGeneratorProperties : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalGeneratorProperties()
        {
            var methodOptions = new SelectPropertyRowOptions(new NormalMethodValues(), "Text", "Value");

            AddSelect("Method", nameof(MaterialNormalProperties.Method), methodOptions, "sobel3");
            AddText<decimal?>("Strength", nameof(MaterialNormalProperties.Strength), MaterialNormalProperties.DefaultStrength);
        }
    }

    public class OcclusionProperties : PropertyCollectionBase<MaterialOcclusionProperties>
    {
        public OcclusionProperties()
        {
            AddText<string>("Texture", nameof(MaterialOcclusionProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialOcclusionProperties.Value), 0m);
            //Add<decimal?>("Shift", nameof(MaterialOcclusionProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialOcclusionProperties.Scale), 1m);
        }
    }

    public class OcclusionGeneratorProperties : PropertyCollectionBase<MaterialOcclusionProperties>
    {
        public OcclusionGeneratorProperties()
        {
            AddText<decimal?>("Step Length", nameof(MaterialOcclusionProperties.StepDistance), MaterialOcclusionProperties.DefaultStepDistance);
            AddText<decimal?>("Z Bias", nameof(MaterialOcclusionProperties.ZBias), MaterialOcclusionProperties.DefaultZBias);
            AddText<decimal?>("Z Scale", nameof(MaterialOcclusionProperties.ZScale), MaterialOcclusionProperties.DefaultZScale);
        }
    }

    public class SpecularProperties : PropertyCollectionBase<MaterialSpecularProperties>
    {
        public SpecularProperties()
        {
            AddText<string>("Texture", nameof(MaterialSpecularProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSpecularProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSpecularProperties.Scale), 1m);
        }
    }

    public class SmoothProperties : PropertyCollectionBase<MaterialSmoothProperties>
    {
        public SmoothProperties()
        {
            AddText<string>("Texture", nameof(MaterialSmoothProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSmoothProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSmoothProperties.Scale), 1m);
        }
    }

    public class RoughProperties : PropertyCollectionBase<MaterialRoughProperties>
    {
        public RoughProperties()
        {
            AddText<string>("Texture", nameof(MaterialRoughProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialRoughProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialRoughProperties.Scale), 1m);
        }
    }

    public class MetalProperties : PropertyCollectionBase<MaterialMetalProperties>
    {
        public MetalProperties()
        {
            AddText<string>("Texture", nameof(MaterialMetalProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialMetalProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialMetalProperties.Scale), 1m);
        }
    }

    public class F0Properties : PropertyCollectionBase<MaterialF0Properties>
    {
        public F0Properties()
        {
            AddText<string>("Texture", nameof(MaterialF0Properties.Texture));
            AddText<decimal?>("Value", nameof(MaterialF0Properties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialF0Properties.Scale), 1m);
        }
    }

    public class PorosityProperties : PropertyCollectionBase<MaterialPorosityProperties>
    {
        public PorosityProperties()
        {
            AddText<string>("Texture", nameof(MaterialPorosityProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialPorosityProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialPorosityProperties.Scale), 1m);
        }
    }

    public class SssProperties : PropertyCollectionBase<MaterialSssProperties>
    {
        public SssProperties()
        {
            AddText<string>("Texture", nameof(MaterialSssProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSssProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSssProperties.Scale), 1m);
        }
    }

    public class EmissiveProperties : PropertyCollectionBase<MaterialEmissiveProperties>
    {
        public EmissiveProperties()
        {
            AddText<string>("Texture", nameof(MaterialEmissiveProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialEmissiveProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialEmissiveProperties.Scale), 1m);
        }
    }
}
