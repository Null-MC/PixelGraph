using PixelGraph.Common.Material;
using System;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialFiltersControl
    {
        public event EventHandler DataChanged;

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

        private void OnDataChanged(object sender, EventArgs e)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
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
            .Register("Material", typeof(MaterialProperties), typeof(MaterialFiltersControl), new PropertyMetadata(OnMaterialPropertyChanged));

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty
            .Register("SelectedTag", typeof(string), typeof(MaterialFiltersControl), new PropertyMetadata(OnSelectedTagPropertyChanged));
    }
}
