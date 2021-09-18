using PixelGraph.Common.Material;
using SixLabors.ImageSharp;
using System;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialFiltersControl
    {
        public event EventHandler DataChanged;

        public RectangleF? SelectedFilterBounds => (RectangleF?)GetValue(SelectedFilterBoundsProperty);

        public MaterialProperties Material {
            set => SetValue(MaterialProperty, value);
        }

        public string SelectedTag {
            set => SetValue(SelectedTagProperty, value);
        }


        public MaterialFiltersControl()
        {
            InitializeComponent();
        }

        private void InvalidateSelectedFilterBounds()
        {
            if (Model.SelectedFilter != null) {
                Model.SelectedFilter.GetRectangle(out var bounds);
                SetValue(SelectedFilterBoundsProperty, bounds);
                return;
            }

            SetValue(SelectedFilterBoundsProperty, null);
        }

        private void OnDataChanged(object sender, EventArgs e)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            InvalidateSelectedFilterBounds();
        }

        private void OnFilterAddButtonClick(object sender, RoutedEventArgs e)
        {
            Model.AddNewFilter();
        }

        private void OnFilterDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            Model.DeleteSelectedFilter();
        }

        private void OnFilterPropertyChanged(object sender, PropertyGridChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(MaterialFilter.Name):
                    // TODO: update list
                    break;
                case nameof(MaterialFilter.Left):
                case nameof(MaterialFilter.Top):
                case nameof(MaterialFilter.Width):
                case nameof(MaterialFilter.Height):
                    // TODO: update outline
                    InvalidateSelectedFilterBounds();
                    break;
            }
        }

        private static void OnMaterialPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialFiltersControl control) return;
            control.Model.Material = e.NewValue as MaterialProperties;
        }

        private static void OnSelectedTagPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialFiltersControl control) return;
            control.Model.SelectedTag = e.NewValue as string;
        }

        public static readonly DependencyProperty MaterialProperty = DependencyProperty
            .Register(nameof(Material), typeof(MaterialProperties), typeof(MaterialFiltersControl), new PropertyMetadata(OnMaterialPropertyChanged));

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty
            .Register(nameof(SelectedTag), typeof(string), typeof(MaterialFiltersControl), new PropertyMetadata(OnSelectedTagPropertyChanged));

        public static readonly DependencyProperty SelectedFilterBoundsProperty = DependencyProperty
            .Register(nameof(SelectedFilterBounds), typeof(RectangleF?), typeof(MaterialFiltersControl));
    }
}
