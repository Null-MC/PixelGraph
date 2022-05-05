using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Block;
using MinecraftMappings.Internal.Models.Entity;
using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Models;
using PixelGraph.UI.ViewModels;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PixelGraph.UI.Models
{
    internal class MaterialFiltersViewModel : ModelBase
    {
        private MaterialProperties _material;
        private ObservableMaterialFilter _selectedFilter;
        private ObservableCollection<ObservableMaterialFilter> _filterList;
        private ITexturePreviewModel _texturePreviewModel;
        private string _selectedTag;

        public event EventHandler SelectionChanged;
        public event EventHandler DataChanged;

        public FilterGeneralPropertyCollection GeneralProperties {get;}
        public FilterNormalPropertyCollection NormalProperties {get;}

        public bool HasMaterial => _material != null;
        public bool HasSelectedFilter => _selectedFilter != null;
        public bool IsGeneralSelected => TextureTags.Is(_selectedTag, TextureTags.General);
        public bool IsNormalSelected => TextureTags.Is(_selectedTag, TextureTags.Normal);

        public ITexturePreviewModel TexturePreviewData {
            get => _texturePreviewModel;
            set {
                _texturePreviewModel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ObservableMaterialFilter> FilterList {
            get => _filterList;
            private set {
                _filterList = value;
                OnPropertyChanged();
            }
        }

        public MaterialProperties Material {
            get => _material;
            set {
                if (_material == value) return;

                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                UpdateFilterList();
            }
        }

        public ObservableMaterialFilter SelectedFilter {
            get => _selectedFilter;
            set {
                if (_selectedFilter == value) return;

                _selectedFilter = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasSelectedFilter));
                GeneralProperties.SetData(_selectedFilter);
                NormalProperties.SetData(_selectedFilter);

                OnSelectionChanged();
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsGeneralSelected));
                OnPropertyChanged(nameof(IsNormalSelected));
            }
        }


        public MaterialFiltersViewModel()
        {
            GeneralProperties = new FilterGeneralPropertyCollection();
            GeneralProperties.PropertyChanged += OnPropertyValueChanged;

            NormalProperties = new FilterNormalPropertyCollection();
            NormalProperties.PropertyChanged += OnPropertyValueChanged;
        }

        public void ImportFiltersFromModel(ModelLoader loader)
        {
            var entityModel = loader.GetJavaEntityModel(Material);
            if (entityModel != null) {
                var filters = ImportFiltersFromEntityModel(entityModel);
                Material.Filters.AddRange(filters);
                OnDataChanged();
                return;
            }

            var blockModel = loader.GetBlockModel(Material);
            if (blockModel != null) {
                var filters = ImportFiltersFromBlockModel(blockModel);
                Material.Filters.AddRange(filters);
                OnDataChanged();
                return;
            }
            
            throw new ApplicationException("No model found!");
        }

        public void UpdateFilterList()
        {
            _filterList = null;

            if (_material?.Filters != null) {
                var filters = _material.Filters.Select(f => new ObservableMaterialFilter(f));
                _filterList = new ObservableCollection<ObservableMaterialFilter>(filters);
            }

            OnPropertyChanged(nameof(FilterList));
        }

        private static IEnumerable<MaterialFilter> ImportFiltersFromEntityModel(EntityModelVersion model)
        {
            var existingRegions = new List<RectangleF>();
            var nameBuilder = new StringBuilder();

            IEnumerable<MaterialFilter> ProcessElements(IEnumerable<EntityElement> elements) {
                foreach (var element in elements) {
                    if (element.Boxes != null) {
                        var cubeIndex = 1;

                        foreach (var cube in element.Boxes) {
                            foreach (var face in ModelElement.AllFaces) {
                                var region = cube.GetFaceRectangle(face, element.MirrorTexU);

                                if (existingRegions.Contains(region)) continue;
                                existingRegions.Add(region);

                                var faceName = UVHelper.GetFaceName(face);

                                nameBuilder.Clear();

                                if (!string.IsNullOrWhiteSpace(element.Id))
                                    nameBuilder.Append(element.Id);

                                if (cubeIndex > 1)
                                    nameBuilder.Append(cubeIndex);

                                nameBuilder.Append('-');
                                nameBuilder.Append(faceName);

                                var left = region.Left / model.TextureSize.X;
                                var width = region.Width / model.TextureSize.X;

                                if (width < 0f) {
                                    width = -width;
                                    left -= width;
                                }

                                var top = region.Top / model.TextureSize.Y;
                                var height = region.Height / model.TextureSize.Y;

                                if (height < 0f) {
                                    height = -height;
                                    top -= height;
                                }

                                yield return new MaterialFilter {
                                    Name = $"{nameBuilder}",
                                    Top = new decimal(top),
                                    Left = new decimal(left),
                                    Width = new decimal(width),
                                    Height = new decimal(height),
                                };
                            }

                            cubeIndex++;
                        }
                    }

                    if (element.Submodels == null) continue;
                    foreach (var filter in ProcessElements(element.Submodels))
                        yield return filter;
                }
            }

            return ProcessElements(model.Elements);
        }

        private static IEnumerable<MaterialFilter> ImportFiltersFromBlockModel(BlockModelVersion model)
        {
            var existingRegions = new List<RectangleF>();

            IEnumerable<MaterialFilter> ProcessElements(IEnumerable<ModelElement> elements) {
                foreach (var element in elements) {
                    foreach (var face in ModelElement.AllFaces) {
                        // Skip if no face data
                        var faceData = element.GetFace(face);
                        if (faceData == null) continue;
                        
                        var rotation = faceData.Rotation ?? 0;

                        RectangleF uv;
                        if (faceData.UV.HasValue) uv = faceData.UV.Value;
                        else UVHelper.GetDefaultUv(element, in face, out uv);

                        UVHelper.GetRotatedRegion(in uv, in rotation, out var region);

                        if (existingRegions.Contains(region)) continue;
                        existingRegions.Add(region);

                        var left = region.Left / 16f;
                        var width = region.Width / 16f;

                        if (width < 0f) {
                            width = -width;
                            left -= width;
                        }

                        var top = region.Top / 16f;
                        var height = region.Height / 16f;

                        if (height < 0f) {
                            height = -height;
                            top -= height;
                        }

                        yield return new MaterialFilter {
                            Name = UVHelper.GetElementFaceName(in element.Name, in face),
                            Top = new decimal(top),
                            Left = new decimal(left),
                            Width = new decimal(width),
                            Height = new decimal(height),
                        };
                    }
                }
            }

            return ProcessElements(model.Elements);
        }

        private void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDataChanged();
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddNewFilter(MaterialFilter newFilter = null)
        {
            newFilter ??= new MaterialFilter();

            _material.Filters ??= new List<MaterialFilter>();
            _material.Filters.Add(newFilter);

            var filterModel = new ObservableMaterialFilter(newFilter);

            if (_filterList == null) {
                _filterList = new ObservableCollection<ObservableMaterialFilter>();
                OnPropertyChanged(nameof(FilterList));
            }

            _filterList.Add(filterModel);
            OnDataChanged();

            SelectedFilter = filterModel;
        }

        public void DeleteSelectedFilter()
        {
            if (_selectedFilter == null) return;

            _material.Filters.Remove(_selectedFilter.Filter);
            _filterList.Remove(_selectedFilter);
            OnDataChanged();

            SelectedFilter = null;
        }
    }

    public class FilterGeneralPropertyCollection : PropertyCollectionBase<ObservableMaterialFilter>
    {
        public FilterGeneralPropertyCollection()
        {
            AddText<string>("Name", nameof(ObservableMaterialFilter.Name));
            AddValue<decimal?>("Left", nameof(ObservableMaterialFilter.Left));
            AddValue<decimal?>("Top", nameof(ObservableMaterialFilter.Top));
            AddValue<decimal?>("Width", nameof(ObservableMaterialFilter.Width));
            AddValue<decimal?>("Height", nameof(ObservableMaterialFilter.Height));
            AddBool<bool?>("Tile", nameof(ObservableMaterialFilter.Tile), false);
        }
    }

    public class FilterNormalPropertyCollection : PropertyCollectionBase<ObservableMaterialFilter>
    {
        public FilterNormalPropertyCollection()
        {
            AddValue<decimal?>("Noise", nameof(ObservableMaterialFilter.NormalNoise));
            //AddSeparator();
            AddValue<decimal?>("Curve X", nameof(ObservableMaterialFilter.NormalCurveX));
            AddValue<decimal?>("Curve Left", nameof(ObservableMaterialFilter.NormalCurveLeft));
            AddValue<decimal?>("Curve Right", nameof(ObservableMaterialFilter.NormalCurveRight));
            AddValue<decimal?>("Curve Y", nameof(ObservableMaterialFilter.NormalCurveY));
            AddValue<decimal?>("Curve Top", nameof(ObservableMaterialFilter.NormalCurveTop));
            AddValue<decimal?>("Curve Bottom", nameof(ObservableMaterialFilter.NormalCurveBottom));
            //AddSeparator();
            AddValue<decimal?>("Radius X", nameof(ObservableMaterialFilter.NormalRadiusX));
            AddValue<decimal?>("Radius Left", nameof(ObservableMaterialFilter.NormalRadiusLeft));
            AddValue<decimal?>("Radius Right", nameof(ObservableMaterialFilter.NormalRadiusRight));
            AddValue<decimal?>("Radius Y", nameof(ObservableMaterialFilter.NormalRadiusY));
            AddValue<decimal?>("Radius Top", nameof(ObservableMaterialFilter.NormalRadiusTop));
            AddValue<decimal?>("Radius Bottom", nameof(ObservableMaterialFilter.NormalRadiusBottom));
        }
    }
}
