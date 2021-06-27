using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using PixelGraph.UI.ViewData;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;

namespace PixelGraph.UI.Models
{
    internal class MaterialPropertiesModel : ModelBase
    {
        private MaterialProperties _material;
        private string _selectedTag;
        private decimal? _iorToF0Value;
        private decimal? _iorDefaultValue;

        public event EventHandler DataChanged;

        public GeneralPropertyCollection GeneralProperties {get;}
        public AlphaPropertyCollection AlphaProperties {get;}
        public AlbedoPropertyCollection AlbedoProperties {get;}
        public DiffusePropertyCollection DiffuseProperties {get;}
        public HeightPropertyCollection Height {get;}
        public HeightEdgeFadingPropertyCollection HeightEdgeFading {get;}
        public NormalPropertyCollection Normal {get;}
        public NormalGeneratorPropertyCollection NormalGeneration {get;}
        public OcclusionPropertyCollection Occlusion {get;}
        public OcclusionGeneratorPropertyCollection OcclusionGeneration {get;}
        public SpecularPropertyCollection Specular {get;}
        public SmoothPropertyCollection Smooth {get;}
        public RoughPropertyCollection Rough {get;}
        public MetalPropertyCollection Metal {get;}
        public F0PropertyCollection F0 {get;}
        public PorosityPropertyCollection Porosity {get;}
        public SssPropertyCollection Sss {get; set;}
        public EmissivePropertyCollection Emissive {get;}

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

        public decimal? IorActualValue => _iorToF0Value;

        public MaterialProperties Material {
            get => _material;
            set {
                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                GeneralProperties.SetData(Material);
                AlphaProperties.SetData(Material?.Alpha);
                AlbedoProperties.SetData(Material?.Albedo);
                DiffuseProperties.SetData(Material?.Diffuse);
                Height.SetData(Material?.Height);
                HeightEdgeFading.SetData(Material?.Height);
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


        public MaterialPropertiesModel()
        {
            GeneralProperties = new GeneralPropertyCollection();
            GeneralProperties.PropertyChanged += OnPropertyValueChanged;

            AlphaProperties = new AlphaPropertyCollection();
            AlphaProperties.PropertyChanged += OnPropertyValueChanged;

            AlbedoProperties = new AlbedoPropertyCollection();
            AlbedoProperties.PropertyChanged += OnPropertyValueChanged;

            DiffuseProperties = new DiffusePropertyCollection();
            DiffuseProperties.PropertyChanged += OnPropertyValueChanged;

            Height = new HeightPropertyCollection();
            Height.PropertyChanged += OnPropertyValueChanged;

            HeightEdgeFading = new HeightEdgeFadingPropertyCollection();
            HeightEdgeFading.PropertyChanged += OnPropertyValueChanged;

            Normal = new NormalPropertyCollection();
            Normal.PropertyChanged += OnPropertyValueChanged;

            NormalGeneration = new NormalGeneratorPropertyCollection();
            NormalGeneration.PropertyChanged += OnPropertyValueChanged;

            Occlusion = new OcclusionPropertyCollection();
            Occlusion.PropertyChanged += OnPropertyValueChanged;

            OcclusionGeneration = new OcclusionGeneratorPropertyCollection();
            OcclusionGeneration.PropertyChanged += OnPropertyValueChanged;

            Specular = new SpecularPropertyCollection();
            Specular.PropertyChanged += OnPropertyValueChanged;

            Smooth = new SmoothPropertyCollection();
            Smooth.PropertyChanged += OnPropertyValueChanged;

            Rough = new RoughPropertyCollection();
            Rough.PropertyChanged += OnPropertyValueChanged;

            Metal = new MetalPropertyCollection();
            Metal.PropertyChanged += OnPropertyValueChanged;

            F0 = new F0PropertyCollection();
            F0.PropertyChanged += OnF0PropertyValueChanged;

            Porosity = new PorosityPropertyCollection();
            Porosity.PropertyChanged += OnPropertyValueChanged;

            Sss = new SssPropertyCollection();
            Sss.PropertyChanged += OnPropertyValueChanged;

            Emissive = new EmissivePropertyCollection();
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

    public class GeneralPropertyCollection : PropertyCollectionBase<MaterialProperties>
    {
        public GeneralPropertyCollection()
        {
            var options = new SelectPropertyRowOptions(new AllTextureFormatValues(), "Text", "Value");

            AddSelect("Input Format", nameof(MaterialProperties.InputFormat), options, MaterialProperties.DefaultInputFormat);
            AddBool<bool?>("Wrap Horizontally", nameof(MaterialProperties.WrapX), MaterialProperties.DefaultWrap);
            AddBool<bool?>("Wrap Vertically", nameof(MaterialProperties.WrapY), MaterialProperties.DefaultWrap);
        }
    }

    public class AlphaPropertyCollection : PropertyCollectionBase<MaterialAlphaProperties>
    {
        public AlphaPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialAlphaProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialAlphaProperties.Value), 255m);
            AddText<decimal?>("Scale", nameof(MaterialAlphaProperties.Scale), 1m);
        }
    }

    public class AlbedoPropertyCollection : PropertyCollectionBase<MaterialAlbedoProperties>
    {
        public AlbedoPropertyCollection()
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

    public class DiffusePropertyCollection : PropertyCollectionBase<MaterialDiffuseProperties>
    {
        public DiffusePropertyCollection()
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

    public class HeightPropertyCollection : PropertyCollectionBase<MaterialHeightProperties>
    {
        public HeightPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialHeightProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialHeightProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialHeightProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialHeightProperties.Scale), 1m);
        }
    }

    public class HeightEdgeFadingPropertyCollection : PropertyCollectionBase<MaterialHeightProperties>
    {
        public HeightEdgeFadingPropertyCollection()
        {
            AddText<decimal?>("Horizontal", nameof(MaterialHeightProperties.EdgeFadeX));
            AddText<decimal?>("Vertical", nameof(MaterialHeightProperties.EdgeFadeY));
        }
    }

    public class NormalPropertyCollection : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialNormalProperties.Texture));
        }
    }

    public class NormalGeneratorPropertyCollection : PropertyCollectionBase<MaterialNormalProperties>
    {
        public NormalGeneratorPropertyCollection()
        {
            var methodOptions = new SelectPropertyRowOptions(new NormalMethodValues(), "Text", "Value");

            AddSelect("Method", nameof(MaterialNormalProperties.Method), methodOptions, "sobel3");
            AddText<decimal?>("Strength", nameof(MaterialNormalProperties.Strength), MaterialNormalProperties.DefaultStrength);
        }
    }

    public class OcclusionPropertyCollection : PropertyCollectionBase<MaterialOcclusionProperties>
    {
        public OcclusionPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialOcclusionProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialOcclusionProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialOcclusionProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialOcclusionProperties.Scale), 1m);
        }
    }

    public class OcclusionGeneratorPropertyCollection : PropertyCollectionBase<MaterialOcclusionProperties>
    {
        public OcclusionGeneratorPropertyCollection()
        {
            AddText<decimal?>("Step Length", nameof(MaterialOcclusionProperties.StepDistance), MaterialOcclusionProperties.DefaultStepDistance);
            AddText<decimal?>("Z Bias", nameof(MaterialOcclusionProperties.ZBias), MaterialOcclusionProperties.DefaultZBias);
            AddText<decimal?>("Z Scale", nameof(MaterialOcclusionProperties.ZScale), MaterialOcclusionProperties.DefaultZScale);
        }
    }

    public class SpecularPropertyCollection : PropertyCollectionBase<MaterialSpecularProperties>
    {
        public SpecularPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialSpecularProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSpecularProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialSpecularProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSpecularProperties.Scale), 1m);
        }
    }

    public class SmoothPropertyCollection : PropertyCollectionBase<MaterialSmoothProperties>
    {
        public SmoothPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialSmoothProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSmoothProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialSmoothProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSmoothProperties.Scale), 1m);
        }
    }

    public class RoughPropertyCollection : PropertyCollectionBase<MaterialRoughProperties>
    {
        public RoughPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialRoughProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialRoughProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialRoughProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialRoughProperties.Scale), 1m);
        }
    }

    public class MetalPropertyCollection : PropertyCollectionBase<MaterialMetalProperties>
    {
        public MetalPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialMetalProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialMetalProperties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialMetalProperties.Scale), 1m);
        }
    }

    public class F0PropertyCollection : PropertyCollectionBase<MaterialF0Properties>
    {
        public F0PropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialF0Properties.Texture));
            AddText<decimal?>("Value", nameof(MaterialF0Properties.Value), 0m);
            AddText<decimal?>("Scale", nameof(MaterialF0Properties.Scale), 1m);
        }
    }

    public class PorosityPropertyCollection : PropertyCollectionBase<MaterialPorosityProperties>
    {
        public PorosityPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialPorosityProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialPorosityProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialPorosityProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialPorosityProperties.Scale), 1m);
        }
    }

    public class SssPropertyCollection : PropertyCollectionBase<MaterialSssProperties>
    {
        public SssPropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialSssProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialSssProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialSssProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialSssProperties.Scale), 1m);
        }
    }

    public class EmissivePropertyCollection : PropertyCollectionBase<MaterialEmissiveProperties>
    {
        public EmissivePropertyCollection()
        {
            AddText<string>("Texture", nameof(MaterialEmissiveProperties.Texture));
            AddText<decimal?>("Value", nameof(MaterialEmissiveProperties.Value), 0m);
            AddText<decimal?>("Shift", nameof(MaterialEmissiveProperties.Shift), 0m);
            AddText<decimal?>("Scale", nameof(MaterialEmissiveProperties.Scale), 1m);
        }
    }
}
