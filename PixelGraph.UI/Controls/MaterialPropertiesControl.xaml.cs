using PixelGraph.Common.Material;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialPropertiesControl
    {
        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;
        public event EventHandler DataChanged;
        public event EventHandler ModelChanged;

        public string ProjectRootPath {
            //get => (string)GetValue(SelectedTagProperty);
            set => SetValue(ProjectRootPathProperty, value);
        }

        public MaterialProperties Material {
            //get => (MaterialProperties)GetValue(MaterialProperty);
            set => SetValue(MaterialProperty, value);
        }

        public string SelectedTag {
            //get => (string)GetValue(SelectedTagProperty);
            set => SetValue(SelectedTagProperty, value);
        }


        public MaterialPropertiesControl()
        {
            InitializeComponent();
        }

        protected virtual void OnDataChanged(object sender, EventArgs e)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnModelChanged()
        {
            ModelChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateNormalClick(object sender, RoutedEventArgs e)
        {
            GenerateNormal?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateOcclusionClick(object sender, RoutedEventArgs e)
        {
            GenerateOcclusion?.Invoke(this, EventArgs.Empty);
        }

        private void OnIorToFoConvertButtonClick(object sender, RoutedEventArgs e)
        {
            Model.ConvertIorToF0();
        }

        private void OnF0ConverterTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                IorToF0ConvertButton.Focus();
                Model.ConvertIorToF0();
            }
        }

        private void OnPropertyGridModelChanged(object sender, EventArgs e)
        {
            OnModelChanged();
        }

        private static void OnProjectRootPathPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialPropertiesControl control) return;

            control.ProjectRootPath = e.NewValue as string;
        }

        private static void OnMaterialPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialPropertiesControl control) return;

            control.Model.Material = e.NewValue as MaterialProperties;
        }

        private static void OnSelectedTagPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialPropertiesControl control) return;

            control.Model.SelectedTag = e.NewValue as string;
        }

        public static readonly DependencyProperty ProjectRootPathProperty = DependencyProperty
            .Register(nameof(ProjectRootPath), typeof(string), typeof(MaterialPropertiesControl), new PropertyMetadata(OnProjectRootPathPropertyChanged));

        public static readonly DependencyProperty MaterialProperty = DependencyProperty
            .Register(nameof(Material), typeof(MaterialProperties), typeof(MaterialPropertiesControl), new PropertyMetadata(OnMaterialPropertyChanged));

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty
            .Register(nameof(SelectedTag), typeof(string), typeof(MaterialPropertiesControl), new PropertyMetadata(OnSelectedTagPropertyChanged));

        public class TextureItem
        {
            public string Name {get; set;}
            public string Key {get; set;}
        }
    }
}
