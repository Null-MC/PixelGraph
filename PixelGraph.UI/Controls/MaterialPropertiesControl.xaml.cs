using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialPropertiesControl
    {
        //private MaterialVM _materialVM;
        //internal MaterialVM vm;

        public event EventHandler DataChanged;
        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;
        //public event PropertyChangedEventHandler PropertyChanged;

        public MaterialProperties Material {
            get => (MaterialProperties)GetValue(MaterialProperty);
            set => SetValue(MaterialProperty, value);
        }

        public string SelectedTag {
            get => (string)GetValue(SelectedTagProperty);
            set => SetValue(SelectedTagProperty, value);
        }

        //public ResourcePackInputProperties PackInput {
        //    get => (ResourcePackInputProperties)GetValue(PackInputProperty);
        //    set => SetValue(PackInputProperty, value);
        //}

        //public string Selected {
        //    get => (string)GetValue(SelectedProperty);
        //    set {
        //        SetValue(SelectedProperty, value);
        //        UpdateDefaults();
        //    }
        //}

        //public bool HasTexture => Material != null;
        //public bool IsGeneralSelected => Selected == null;
        //public bool IsAlbedoSelected => TextureTags.Is(Selected, TextureTags.Albedo);
        //public bool IsHeightSelected => TextureTags.Is(Selected, TextureTags.Height);
        //public bool IsNormalSelected => TextureTags.Is(Selected, TextureTags.Normal);
        //public bool IsOcclusionSelected => TextureTags.Is(Selected, TextureTags.Occlusion);
        //public bool IsSpecularSelected => TextureTags.Is(Selected, TextureTags.Specular);
        //public bool IsSmoothSelected => TextureTags.Is(Selected, TextureTags.Smooth);
        //public bool IsRoughSelected => TextureTags.Is(Selected, TextureTags.Rough);
        //public bool IsMetalSelected => TextureTags.Is(Selected, TextureTags.Metal);
        //public bool IsPorositySelected => TextureTags.Is(Selected, TextureTags.Porosity);
        //public bool IsSssSelected => TextureTags.Is(Selected, TextureTags.SubSurfaceScattering);
        //public bool IsEmissiveSelected => TextureTags.Is(Selected, TextureTags.Emissive);

        //public string DefaultInputFormat => PackInput?.Format ?? MaterialProperties.DefaultInputFormat;
        //public string DefaultOcclusionQuality => MaterialOcclusionProperties.DefaultQuality.ToString("N3");
        //public string DefaultOcclusionSteps => MaterialOcclusionProperties.DefaultSteps.ToString();
        //public string DefaultOcclusionZBias => MaterialOcclusionProperties.DefaultZBias.ToString("N3");
        //public string DefaultOcclusionZScale => MaterialOcclusionProperties.DefaultZScale.ToString("N3");


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
            Add(new MaterialPropertiesControl.TextureItem {Name = "Albedo", Key = TextureTags.Albedo});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Diffuse", Key = TextureTags.Diffuse});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Height", Key = TextureTags.Height});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Normal", Key = TextureTags.Normal});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Occlusion", Key = TextureTags.Occlusion});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Specular", Key = TextureTags.Specular});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Smoothness", Key = TextureTags.Smooth});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Roughness", Key = TextureTags.Rough});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Metal", Key = TextureTags.Metal});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Porosity", Key = TextureTags.Porosity});
            Add(new MaterialPropertiesControl.TextureItem {Name = "SSS", Key = TextureTags.SubSurfaceScattering});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Emissive", Key = TextureTags.Emissive});
        }
    }
}
