using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using System;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.UI.ViewModels
{
    internal class MaterialPropertiesVM : ViewModelBase
    {
        private MaterialProperties _material;
        private ResourcePackInputProperties _packInput;
        private string _selectedTag;

        public event EventHandler DataChanged;

        public bool HasMaterial => _material != null;
        public bool IsGeneralSelected => _selectedTag == null;
        public bool IsAlbedoSelected => TextureTags.Is(_selectedTag, TextureTags.Albedo);
        public bool IsHeightSelected => TextureTags.Is(_selectedTag, TextureTags.Height);
        public bool IsNormalSelected => TextureTags.Is(_selectedTag, TextureTags.Normal);
        public bool IsOcclusionSelected => TextureTags.Is(_selectedTag, TextureTags.Occlusion);
        public bool IsSpecularSelected => TextureTags.Is(_selectedTag, TextureTags.Specular);
        public bool IsSmoothSelected => TextureTags.Is(_selectedTag, TextureTags.Smooth);
        public bool IsRoughSelected => TextureTags.Is(_selectedTag, TextureTags.Rough);
        public bool IsMetalSelected => TextureTags.Is(_selectedTag, TextureTags.Metal);
        public bool IsPorositySelected => TextureTags.Is(_selectedTag, TextureTags.Porosity);
        public bool IsSssSelected => TextureTags.Is(_selectedTag, TextureTags.SubSurfaceScattering);
        public bool IsEmissiveSelected => TextureTags.Is(_selectedTag, TextureTags.Emissive);

        public string DefaultInputFormat => _packInput?.Format ?? MaterialProperties.DefaultInputFormat;
        public string DefaultOcclusionQuality => MaterialOcclusionProperties.DefaultQuality.ToString("N3");
        public string DefaultOcclusionSteps => MaterialOcclusionProperties.DefaultSteps.ToString();
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
                UpdateAlbedoProperties();
                UpdateHeightProperties();
                UpdateNormalProperties();
                UpdateOcclusionProperties();
                UpdateSpecularProperties();
                UpdateSmoothProperties();
                UpdateRoughProperties();
                UpdateMetalProperties();
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

                OnPropertyChanged(nameof(IsGeneralSelected));
                OnPropertyChanged(nameof(IsAlbedoSelected));
                OnPropertyChanged(nameof(IsHeightSelected));
                OnPropertyChanged(nameof(IsNormalSelected));
                OnPropertyChanged(nameof(IsOcclusionSelected));
                OnPropertyChanged(nameof(IsSpecularSelected));
                OnPropertyChanged(nameof(IsSmoothSelected));
                OnPropertyChanged(nameof(IsRoughSelected));
                OnPropertyChanged(nameof(IsMetalSelected));
                OnPropertyChanged(nameof(IsPorositySelected));
                OnPropertyChanged(nameof(IsSssSelected));
                OnPropertyChanged(nameof(IsEmissiveSelected));
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

        public bool? Wrap {
            get => _material?.Wrap;
            set {
                _material.Wrap = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateGeneralProperties()
        {
            OnPropertyChanged(nameof(InputFormat));
            OnPropertyChanged(nameof(Wrap));
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

        public byte? AlbedoValueRed {
            get => _material?.Albedo?.ValueRed;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueRed = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoScaleRed {
            get => _material?.Albedo?.ScaleRed;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleRed = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public byte? AlbedoValueGreen {
            get => _material?.Albedo?.ValueGreen;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueGreen = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoScaleGreen {
            get => _material?.Albedo?.ScaleGreen;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleGreen = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public byte? AlbedoValueBlue {
            get => _material?.Albedo?.ValueBlue;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueBlue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoScaleBlue {
            get => _material?.Albedo?.ScaleBlue;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleBlue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public byte? AlbedoValueAlpha {
            get => _material?.Albedo?.ValueAlpha;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ValueAlpha = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? AlbedoScaleAlpha {
            get => _material?.Albedo?.ScaleAlpha;
            set {
                _material.Albedo ??= new MaterialAlbedoProperties();
                _material.Albedo.ScaleAlpha = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void UpdateAlbedoProperties()
        {
            OnPropertyChanged(nameof(AlbedoTexture));
            OnPropertyChanged(nameof(AlbedoValueRed));
            OnPropertyChanged(nameof(AlbedoValueGreen));
            OnPropertyChanged(nameof(AlbedoValueBlue));
            OnPropertyChanged(nameof(AlbedoValueAlpha));
            OnPropertyChanged(nameof(AlbedoScaleRed));
            OnPropertyChanged(nameof(AlbedoScaleGreen));
            OnPropertyChanged(nameof(AlbedoScaleBlue));
            OnPropertyChanged(nameof(AlbedoScaleAlpha));
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

        public byte? HeightValue {
            get => _material?.Height?.Value;
            set {
                _material.Height ??= new MaterialHeightProperties();
                _material.Height.Value = value;
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

        private void UpdateHeightProperties()
        {
            OnPropertyChanged(nameof(HeightTexture));
            OnPropertyChanged(nameof(HeightValue));
            OnPropertyChanged(nameof(HeightScale));
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

        public byte? NormalValueX {
            get => _material?.Normal?.ValueX;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.ValueX = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public byte? NormalValueY {
            get => _material?.Normal?.ValueY;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.ValueY = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public byte? NormalValueZ {
            get => _material?.Normal?.ValueZ;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.ValueZ = value;
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

        public decimal? NormalNoise {
            get => _material?.Normal?.Noise;
            set {
                _material.Normal ??= new MaterialNormalProperties();
                _material.Normal.Noise = value;
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
            OnPropertyChanged(nameof(NormalStrength));
            OnPropertyChanged(nameof(NormalNoise));
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

        public byte? OcclusionValue {
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

        public int? OcclusionSteps {
            get => _material?.Occlusion?.Steps;
            set {
                _material.Occlusion ??= new MaterialOcclusionProperties();
                _material.Occlusion.Steps = value;
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
            OnPropertyChanged(nameof(OcclusionSteps));
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

        public byte? SmoothValue {
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

        public byte? RoughValue {
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

        public byte? MetalValue {
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

        public byte? PorosityValue {
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

        public byte? SssValue {
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

        public byte? EmissiveValue {
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
