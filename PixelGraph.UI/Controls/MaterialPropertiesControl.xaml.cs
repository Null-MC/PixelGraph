using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialPropertiesControl
    {
        public event EventHandler DataChanged;
        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;

        public MaterialProperties Material {
            get => (MaterialProperties)GetValue(MaterialProperty);
            set => SetValue(MaterialProperty, value);
        }

        public string SelectedTag {
            get => (string)GetValue(SelectedTagProperty);
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

        private static void OnMaterialPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is MaterialPropertiesControl control)) return;

            control.vm.Material = e.NewValue as MaterialProperties;
        }

        private static void OnSelectedTagPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is MaterialPropertiesControl control)) return;

            control.vm.SelectedTag = e.NewValue as string;
        }

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

    public class TextureItemList : List<MaterialPropertiesControl.TextureItem>
    {
        public TextureItemList()
        {
            Add(new MaterialPropertiesControl.TextureItem {Name = "General"});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Alpha", Key = TextureTags.Alpha});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Albedo", Key = TextureTags.Albedo});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Diffuse", Key = TextureTags.Diffuse});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Height", Key = TextureTags.Height});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Normal", Key = TextureTags.Normal});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Occlusion", Key = TextureTags.Occlusion});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Specular", Key = TextureTags.Specular});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Smoothness", Key = TextureTags.Smooth});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Roughness", Key = TextureTags.Rough});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Metal", Key = TextureTags.Metal});
            Add(new MaterialPropertiesControl.TextureItem {Name = "F0", Key = TextureTags.F0});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Porosity", Key = TextureTags.Porosity});
            Add(new MaterialPropertiesControl.TextureItem {Name = "SSS", Key = TextureTags.SubSurfaceScattering});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Emissive", Key = TextureTags.Emissive});
        }
    }
}
