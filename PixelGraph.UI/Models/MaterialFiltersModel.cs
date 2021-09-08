using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PixelGraph.UI.Models
{
    internal class MaterialFiltersModel : ModelBase
    {
        private MaterialProperties _material;
        private MaterialFilter _selectedFilter;
        private ObservableCollection<MaterialFilter> _filterList;
        private string _selectedTag;

        public event EventHandler DataChanged;

        public FilterGeneralPropertyCollection GeneralProperties {get;}
        public FilterNormalPropertyCollection NormalProperties {get;}

        public bool HasMaterial => _material != null;
        public bool IsGeneralSelected => TextureTags.Is(_selectedTag, TextureTags.General);
        public bool IsNormalSelected => TextureTags.Is(_selectedTag, TextureTags.Normal);

        public ObservableCollection<MaterialFilter> FilterList {
            get => _filterList;
            private set {
                _filterList = value;
                OnPropertyChanged();
            }
        }

        public MaterialProperties Material {
            get => _material;
            set {
                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                UpdateFilterList();
            }
        }

        public MaterialFilter SelectedFilter {
            get => _selectedFilter;
            set {
                _selectedFilter = value;
                OnPropertyChanged();

                GeneralProperties.SetData(value);
                NormalProperties.SetData(value);
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


        public MaterialFiltersModel()
        {
            //FilterList = new ObservableCollection<MaterialFilter>();

            GeneralProperties = new FilterGeneralPropertyCollection();
            GeneralProperties.PropertyChanged += OnPropertyValueChanged;

            NormalProperties = new FilterNormalPropertyCollection();
            NormalProperties.PropertyChanged += OnPropertyValueChanged;
        }

        private void UpdateFilterList()
        {
            FilterList = null;

            if (_material?.Filters == null) return;
            FilterList = new ObservableCollection<MaterialFilter>(_material.Filters);
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

    public class FilterGeneralPropertyCollection : PropertyCollectionBase<MaterialFilter>
    {
        public FilterGeneralPropertyCollection()
        {
            AddText<string>("Name", nameof(MaterialFilter.Name));
            AddText<decimal?>("Left", nameof(MaterialFilter.Left));
            AddText<decimal?>("Top", nameof(MaterialFilter.Top));
            AddText<decimal?>("Width", nameof(MaterialFilter.Width));
            AddText<decimal?>("Height", nameof(MaterialFilter.Height));
            AddBool<bool?>("Tile", nameof(MaterialFilter.Tile), false);
        }
    }

    public class FilterNormalPropertyCollection : PropertyCollectionBase<MaterialFilter>
    {
        public FilterNormalPropertyCollection()
        {
            AddText<decimal?>("Noise", nameof(MaterialFilter.NormalNoise));
            //AddSeparator();
            AddText<decimal?>("Curve X", nameof(MaterialFilter.NormalCurveX));
            AddText<decimal?>("Curve Left", nameof(MaterialFilter.NormalCurveLeft));
            AddText<decimal?>("Curve Right", nameof(MaterialFilter.NormalCurveRight));
            AddText<decimal?>("Curve Y", nameof(MaterialFilter.NormalCurveY));
            AddText<decimal?>("Curve Top", nameof(MaterialFilter.NormalCurveTop));
            AddText<decimal?>("Curve Bottom", nameof(MaterialFilter.NormalCurveBottom));
            //AddSeparator();
            AddText<decimal?>("Radius X", nameof(MaterialFilter.NormalRadiusX));
            AddText<decimal?>("Radius Left", nameof(MaterialFilter.NormalRadiusLeft));
            AddText<decimal?>("Radius Right", nameof(MaterialFilter.NormalRadiusRight));
            AddText<decimal?>("Radius Y", nameof(MaterialFilter.NormalRadiusY));
            AddText<decimal?>("Radius Top", nameof(MaterialFilter.NormalRadiusTop));
            AddText<decimal?>("Radius Bottom", nameof(MaterialFilter.NormalRadiusBottom));
        }
    }
}
