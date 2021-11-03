﻿using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PixelGraph.UI.Models
{
    internal class MaterialFiltersModel : ModelBase
    {
        private MaterialProperties _material;
        private ObservableMaterialFilter _selectedFilter;
        private ObservableCollection<ObservableMaterialFilter> _filterList;
        private string _selectedTag;

        public event EventHandler SelectionChanged;
        public event EventHandler DataChanged;

        public FilterGeneralPropertyCollection GeneralProperties {get;}
        public FilterNormalPropertyCollection NormalProperties {get;}

        public bool HasMaterial => _material != null;
        public bool HasSelectedFilter => _selectedFilter != null;
        public bool IsGeneralSelected => TextureTags.Is(_selectedTag, TextureTags.General);
        public bool IsNormalSelected => TextureTags.Is(_selectedTag, TextureTags.Normal);

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


        public MaterialFiltersModel()
        {
            GeneralProperties = new FilterGeneralPropertyCollection();
            GeneralProperties.PropertyChanged += OnPropertyValueChanged;

            NormalProperties = new FilterNormalPropertyCollection();
            NormalProperties.PropertyChanged += OnPropertyValueChanged;
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
            AddText<decimal?>("Left", nameof(ObservableMaterialFilter.Left));
            AddText<decimal?>("Top", nameof(ObservableMaterialFilter.Top));
            AddText<decimal?>("Width", nameof(ObservableMaterialFilter.Width));
            AddText<decimal?>("Height", nameof(ObservableMaterialFilter.Height));
            AddBool<bool?>("Tile", nameof(ObservableMaterialFilter.Tile), false);
        }
    }

    public class FilterNormalPropertyCollection : PropertyCollectionBase<ObservableMaterialFilter>
    {
        public FilterNormalPropertyCollection()
        {
            AddText<decimal?>("Noise", nameof(ObservableMaterialFilter.NormalNoise));
            //AddSeparator();
            AddText<decimal?>("Curve X", nameof(ObservableMaterialFilter.NormalCurveX));
            AddText<decimal?>("Curve Left", nameof(ObservableMaterialFilter.NormalCurveLeft));
            AddText<decimal?>("Curve Right", nameof(ObservableMaterialFilter.NormalCurveRight));
            AddText<decimal?>("Curve Y", nameof(ObservableMaterialFilter.NormalCurveY));
            AddText<decimal?>("Curve Top", nameof(ObservableMaterialFilter.NormalCurveTop));
            AddText<decimal?>("Curve Bottom", nameof(ObservableMaterialFilter.NormalCurveBottom));
            //AddSeparator();
            AddText<decimal?>("Radius X", nameof(ObservableMaterialFilter.NormalRadiusX));
            AddText<decimal?>("Radius Left", nameof(ObservableMaterialFilter.NormalRadiusLeft));
            AddText<decimal?>("Radius Right", nameof(ObservableMaterialFilter.NormalRadiusRight));
            AddText<decimal?>("Radius Y", nameof(ObservableMaterialFilter.NormalRadiusY));
            AddText<decimal?>("Radius Top", nameof(ObservableMaterialFilter.NormalRadiusTop));
            AddText<decimal?>("Radius Bottom", nameof(ObservableMaterialFilter.NormalRadiusBottom));
        }
    }
}