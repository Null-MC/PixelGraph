using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class MaterialPropertiesVM : ViewModelBase
    {
        private MaterialProperties _material;
        private ResourcePackInputProperties _packInput;
        private string _selectedTag;

        public event EventHandler DataChanged;

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

        public string DefaultInputFormat => _packInput?.Format ?? MaterialProperties.DefaultInputFormat;
        public string DefaultNormalFilter => MaterialNormalProperties.DefaultFilter.ToString();
        //public string DefaultOcclusionSampler => MaterialOcclusionProperties.DefaultSampler;
        public string DefaultOcclusionQuality => MaterialOcclusionProperties.DefaultQuality.ToString("N3");
        public string DefaultOcclusionStepDistance => MaterialOcclusionProperties.DefaultStepDistance.ToString("N3");
        public string DefaultOcclusionZBias => MaterialOcclusionProperties.DefaultZBias.ToString("N3");
        public string DefaultOcclusionZScale => MaterialOcclusionProperties.DefaultZScale.ToString("N3");

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                _packInput = value;
                OnPropertyChanged();
            }
        }

        public MaterialProperties Material {
            get => _material;
            set {
                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                UpdateGeneralProperties();
                UpdateAlphaProperties();
                UpdateAlbedoProperties();
                UpdateDiffuseProperties();
                UpdateHeightProperties();
                UpdateNormalProperties();
                UpdateOcclusionProperties();
                UpdateSpecularProperties();
                UpdateSmoothProperties();
                UpdateRoughProperties();
                UpdateMetalProperties();
                UpdateF0Properties();
                UpdatePorosityProperties();
                UpdateSssProperties();
                UpdateEmissiveProperties();
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


        //public MaterialPropertiesVM()
        //{
        //    _selectedTag = TextureTags.Albedo;
        //}

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

        #region Alpha

        public string AlphaTexture {
            get => _material?.Alpha?.Texture;
            set {
                _material.Alpha ??= new MaterialAlphaProperties();
                _material.Alpha.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlphaValue {
            get => _material?.Alpha?.Value;
            set {
                _material.Alpha ??= new MaterialAlphaProperties();
                _material.Alpha.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlphaScale {
            get => _material?.Alpha?.Scale;
            set {
                _material.Alpha ??= new MaterialAlphaProperties();
                _material.Alpha.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateAlphaProperties()
        {
            OnPropertyChanged(nameof(AlphaTexture));
            OnPropertyChanged(nameof(AlphaValue));
            OnPropertyChanged(nameof(AlphaScale));
        }

        #endregion

        #region Diffuse

        public string DiffuseTexture {
            get => _material?.Diffuse?.Texture;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? DiffuseRedValue {
            get => _material?.Diffuse?.ValueRed;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.ValueRed = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? DiffuseRedScale {
            get => _material?.Diffuse?.ScaleRed;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.ScaleRed = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? DiffuseGreenValue {
            get => _material?.Diffuse?.ValueGreen;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.ValueGreen = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? DiffuseGreenScale {
            get => _material?.Diffuse?.ScaleGreen;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.ScaleGreen = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? DiffuseBlueValue {
            get => _material?.Diffuse?.ValueBlue;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.ValueBlue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? DiffuseBlueScale {
            get => _material?.Diffuse?.ScaleBlue;
            set {
                _material.Diffuse ??= new MaterialDiffuseProperties();
                _material.Diffuse.ScaleBlue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateDiffuseProperties()
        {
            OnPropertyChanged(nameof(DiffuseTexture));
            OnPropertyChanged(nameof(DiffuseRedValue));
            OnPropertyChanged(nameof(DiffuseGreenValue));
            OnPropertyChanged(nameof(DiffuseBlueValue));
            OnPropertyChanged(nameof(DiffuseRedScale));
            OnPropertyChanged(nameof(DiffuseGreenScale));
            OnPropertyChanged(nameof(DiffuseBlueScale));
        }

        #endregion

        #region Albedo

        public string AlbedoTexture {
            get => _material?.Albedo?.Texture;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoRedValue {
            get => _material?.Albedo?.ValueRed;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueRed = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoRedScale {
            get => _material?.Albedo?.ScaleRed;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleRed = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoGreenValue {
            get => _material?.Albedo?.ValueGreen;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueGreen = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoGreenScale {
            get => _material?.Albedo?.ScaleGreen;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleGreen = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoBlueValue {
            get => _material?.Albedo?.ValueBlue;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueBlue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoBlueScale {
            get => _material?.Albedo?.ScaleBlue;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleBlue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateAlbedoProperties()
        {
            OnPropertyChanged(nameof(AlbedoTexture));
            OnPropertyChanged(nameof(AlbedoRedValue));
            OnPropertyChanged(nameof(AlbedoGreenValue));
            OnPropertyChanged(nameof(AlbedoBlueValue));
            OnPropertyChanged(nameof(AlbedoRedScale));
            OnPropertyChanged(nameof(AlbedoGreenScale));
            OnPropertyChanged(nameof(AlbedoBlueScale));
        }

        #endregion

        #region Height

        public string HeightTexture {
            get => _material?.Height?.Texture;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? HeightValue {
            get => _material?.Height?.Value;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? HeightShift {
            get => _material?.Height?.Shift;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.Shift = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? HeightScale {
            get => _material?.Height?.Scale;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? HeightEdgeFadeSizeX {
            get => _material?.Height?.EdgeFadeSizeX;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.EdgeFadeSizeX = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? HeightEdgeFadeSizeY {
            get => _material?.Height?.EdgeFadeSizeY;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.EdgeFadeSizeY = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateHeightProperties()
        {
            OnPropertyChanged(nameof(HeightTexture));
            OnPropertyChanged(nameof(HeightValue));
            OnPropertyChanged(nameof(HeightShift));
            OnPropertyChanged(nameof(HeightScale));
            OnPropertyChanged(nameof(HeightEdgeFadeSizeX));
            OnPropertyChanged(nameof(HeightEdgeFadeSizeY));
        }

        #endregion

        #region Normal

        public string NormalTexture {
            get => _material?.Normal?.Texture;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalValueX {
            get => _material?.Normal?.ValueX;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.ValueX = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalValueY {
            get => _material?.Normal?.ValueY;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.ValueY = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalValueZ {
            get => _material?.Normal?.ValueZ;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.ValueZ = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalNoise {
            get => _material?.Normal?.Noise;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.Noise = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalCurveX {
            get => _material?.Normal?.CurveX;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.CurveX = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalCurveY {
            get => _material?.Normal?.CurveY;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.CurveY = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public NormalMapFilters? NormalFilter {
            get => _material?.Normal?.Filter;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.Filter = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? NormalStrength {
            get => _material?.Normal?.Strength;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.Strength = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateNormalProperties()
        {
            OnPropertyChanged(nameof(NormalTexture));
            OnPropertyChanged(nameof(NormalValueX));
            OnPropertyChanged(nameof(NormalValueY));
            OnPropertyChanged(nameof(NormalValueZ));
            OnPropertyChanged(nameof(NormalNoise));
            OnPropertyChanged(nameof(NormalCurveX));
            OnPropertyChanged(nameof(NormalCurveY));
            OnPropertyChanged(nameof(NormalFilter));
            OnPropertyChanged(nameof(NormalStrength));
        }

        #endregion

        #region Occlusion

        public string OcclusionTexture {
            get => _material?.Occlusion?.Texture;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? OcclusionValue {
            get => _material?.Occlusion?.Value;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? OcclusionScale {
            get => _material?.Occlusion?.Scale;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        //public string OcclusionSampler {
        //    get => _material?.Occlusion?.Sampler;
        //    set {
        //        _material.Occlusion ??= new MaterialOcclusionProperties();
        //        _material.Occlusion.Sampler = value;
        //        OnPropertyChanged();
        //        OnDataChanged();
        //    }
        //}

        public decimal? OcclusionQuality {
            get => _material?.Occlusion?.Quality;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.Quality = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? OcclusionZBias {
            get => _material?.Occlusion?.ZBias;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.ZBias = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? OcclusionZScale {
            get => _material?.Occlusion?.ZScale;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.ZScale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public float? OcclusionStepDistance {
            get => _material?.Occlusion?.StepDistance;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.StepDistance = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? OcclusionClipEmissive {
            get => _material?.Occlusion?.ClipEmissive;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.ClipEmissive = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateOcclusionProperties()
        {
            OnPropertyChanged(nameof(OcclusionTexture));
            OnPropertyChanged(nameof(OcclusionValue));
            OnPropertyChanged(nameof(OcclusionScale));
            OnPropertyChanged(nameof(OcclusionQuality));
            OnPropertyChanged(nameof(OcclusionStepDistance));
            OnPropertyChanged(nameof(OcclusionZBias));
            OnPropertyChanged(nameof(OcclusionZScale));
            OnPropertyChanged(nameof(OcclusionClipEmissive));
        }

        #endregion

        #region Specular

        public string SpecularTexture {
            get => _material?.Specular?.Texture;
            set {
                _material.Specular ??= new MaterialSpecularProperties();
                _material.Specular.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateSpecularProperties()
        {
            OnPropertyChanged(nameof(SpecularTexture));
        }

        #endregion

        #region Smooth

        public string SmoothTexture {
            get => _material?.Smooth?.Texture;
            set {
                _material.Smooth ??= new MaterialSmoothProperties();
                _material.Smooth.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? SmoothValue {
            get => _material?.Smooth?.Value;
            set {
                _material.Smooth ??= new MaterialSmoothProperties();
                _material.Smooth.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? SmoothScale {
            get => _material?.Smooth?.Scale;
            set {
                _material.Smooth ??= new MaterialSmoothProperties();
                _material.Smooth.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateSmoothProperties()
        {
            OnPropertyChanged(nameof(SmoothTexture));
            OnPropertyChanged(nameof(SmoothValue));
            OnPropertyChanged(nameof(SmoothScale));
        }

        #endregion

        #region Rough

        public string RoughTexture {
            get => _material?.Rough?.Texture;
            set {
                _material.Rough ??= new MaterialRoughProperties();
                _material.Rough.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? RoughValue {
            get => _material?.Rough?.Value;
            set {
                _material.Rough ??= new MaterialRoughProperties();
                _material.Rough.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? RoughScale {
            get => _material?.Rough?.Scale;
            set {
                _material.Rough ??= new MaterialRoughProperties();
                _material.Rough.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateRoughProperties()
        {
            OnPropertyChanged(nameof(RoughTexture));
            OnPropertyChanged(nameof(RoughValue));
            OnPropertyChanged(nameof(RoughScale));
        }

        #endregion

        #region Metal

        public string MetalTexture {
            get => _material?.Metal?.Texture;
            set {
                _material.Metal ??= new MaterialMetalProperties();
                _material.Metal.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? MetalValue {
            get => _material?.Metal?.Value;
            set {
                _material.Metal ??= new MaterialMetalProperties();
                _material.Metal.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? MetalScale {
            get => _material?.Metal?.Scale;
            set {
                _material.Metal ??= new MaterialMetalProperties();
                _material.Metal.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateMetalProperties()
        {
            OnPropertyChanged(nameof(MetalTexture));
            OnPropertyChanged(nameof(MetalValue));
            OnPropertyChanged(nameof(MetalScale));
        }

        #endregion

        #region F0

        public string F0Texture {
            get => _material?.F0?.Texture;
            set {
                _material.F0 ??= new MaterialF0Properties();
                _material.F0.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? F0Value {
            get => _material?.F0?.Value;
            set {
                _material.F0 ??= new MaterialF0Properties();
                _material.F0.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? F0Scale {
            get => _material?.F0?.Scale;
            set {
                _material.F0 ??= new MaterialF0Properties();
                _material.F0.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateF0Properties()
        {
            OnPropertyChanged(nameof(F0Texture));
            OnPropertyChanged(nameof(F0Value));
            OnPropertyChanged(nameof(F0Scale));
        }

        #endregion

        #region Porosity

        public string PorosityTexture {
            get => _material?.Porosity?.Texture;
            set {
                _material.Porosity ??= new MaterialPorosityProperties();
                _material.Porosity.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? PorosityValue {
            get => _material?.Porosity?.Value;
            set {
                _material.Porosity ??= new MaterialPorosityProperties();
                _material.Porosity.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? PorosityScale {
            get => _material?.Porosity?.Scale;
            set {
                _material.Porosity ??= new MaterialPorosityProperties();
                _material.Porosity.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdatePorosityProperties()
        {
            OnPropertyChanged(nameof(PorosityTexture));
            OnPropertyChanged(nameof(PorosityValue));
            OnPropertyChanged(nameof(PorosityScale));
        }

        #endregion

        #region SSS

        public string SssTexture {
            get => _material?.SSS?.Texture;
            set {
                _material.SSS ??= new MaterialSssProperties();
                _material.SSS.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? SssValue {
            get => _material?.SSS?.Value;
            set {
                _material.SSS ??= new MaterialSssProperties();
                _material.SSS.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? SssScale {
            get => _material?.SSS?.Scale;
            set {
                _material.SSS ??= new MaterialSssProperties();
                _material.SSS.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateSssProperties()
        {
            OnPropertyChanged(nameof(SssTexture));
            OnPropertyChanged(nameof(SssValue));
            OnPropertyChanged(nameof(SssScale));
        }

        #endregion

        #region Emissive

        public string EmissiveTexture {
            get => _material?.Emissive?.Texture;
            set {
                _material.Emissive ??= new MaterialEmissiveProperties();
                _material.Emissive.Texture = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? EmissiveValue {
            get => _material?.Emissive?.Value;
            set {
                _material.Emissive ??= new MaterialEmissiveProperties();
                _material.Emissive.Value = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? EmissiveScale {
            get => _material?.Emissive?.Scale;
            set {
                _material.Emissive ??= new MaterialEmissiveProperties();
                _material.Emissive.Scale = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateEmissiveProperties()
        {
            OnPropertyChanged(nameof(EmissiveTexture));
            OnPropertyChanged(nameof(EmissiveValue));
            OnPropertyChanged(nameof(EmissiveScale));
        }

        #endregion

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
