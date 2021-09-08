using PixelGraph.Common.Material;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialPropertiesControl
    {
        public event EventHandler DataChanged;
        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;

        //public MainWindowModel BaseModel {
        //    //get => (MaterialProperties)GetValue(MaterialProperty);
        //    set => SetValue(BaseModelProperty, value);
        //}

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

        private void OnGenerateNormalClick(object sender, RoutedEventArgs e)
        {
            GenerateNormal?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateOcclusionClick(object sender, RoutedEventArgs e)
        {
            GenerateOcclusion?.Invoke(this, EventArgs.Empty);
        }

        private void OnDataChanged(object sender, EventArgs e)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
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

        //private static void OnBaseModelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (sender is not MaterialPropertiesControl control) return;

        //    control.Model.BaseModel = e.NewValue as MainWindowModel;
        //}

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

        //public static readonly DependencyProperty BaseModelProperty = DependencyProperty
        //    .Register(nameof(BaseModel), typeof(MainWindowModel), typeof(MaterialFiltersControl), new PropertyMetadata(OnBaseModelPropertyChanged));

        public static readonly DependencyProperty MaterialProperty = DependencyProperty
            .Register("Material", typeof(MaterialProperties), typeof(MaterialPropertiesControl), new PropertyMetadata(OnMaterialPropertyChanged));

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty
            .Register("SelectedTag", typeof(string), typeof(MaterialPropertiesControl), new PropertyMetadata(OnSelectedTagPropertyChanged));

        public class TextureItem
        {
            public string Name {get; set;}
            public string Key {get; set;}
        }
    }
}
