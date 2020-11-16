using PixelGraph.Common;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class PbrPropertiesControl : INotifyPropertyChanged
    {
        public event EventHandler DataChanged;
        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;
        public event PropertyChangedEventHandler PropertyChanged;

        public PbrProperties Texture {
            get => (PbrProperties)GetValue(TextureProperty);
            set => SetValue(TextureProperty, value);
        }

        public string Selected {
            get => (string)GetValue(SelectedProperty);
            set => SetValue(SelectedProperty, value);
        }

        public bool IsGeneralSelected => Selected == null;
        public bool IsAlbedoSelected => TextureTags.Is(Selected, TextureTags.Albedo);
        public bool IsHeightSelected => TextureTags.Is(Selected, TextureTags.Height);
        public bool IsNormalSelected => TextureTags.Is(Selected, TextureTags.Normal);
        public bool IsOcclusionSelected => TextureTags.Is(Selected, TextureTags.Occlusion);
        public bool IsSpecularSelected => TextureTags.Is(Selected, TextureTags.Specular);
        public bool IsSmoothSelected => TextureTags.Is(Selected, TextureTags.Smooth);
        public bool IsRoughSelected => TextureTags.Is(Selected, TextureTags.Rough);
        public bool IsMetalSelected => TextureTags.Is(Selected, TextureTags.Metal);
        public bool IsPorositySelected => TextureTags.Is(Selected, TextureTags.Porosity);
        public bool IsSssSelected => TextureTags.Is(Selected, TextureTags.SubSurfaceScattering);
        public bool IsEmissiveSelected => TextureTags.Is(Selected, TextureTags.Emissive);


        public PbrPropertiesControl()
        {
            InitializeComponent();
        }

        private void OnTexturePropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateNormalClick(object sender, RoutedEventArgs e)
        {
            GenerateNormal?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateOcclusionClick(object sender, RoutedEventArgs e)
        {
            GenerateOcclusion?.Invoke(this, EventArgs.Empty);
        }

        private static void OnTexturePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is PbrPropertiesControl control)) return;

            if (e.OldValue is PbrProperties oldProperties) {
                oldProperties.PropertyChanged -= control.OnTexturePropertyValueChanged;
            }
            if (e.NewValue is PbrProperties newProperties) {
                newProperties.PropertyChanged += control.OnTexturePropertyValueChanged;
            }
        }

        private static void OnSelectedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is PbrPropertiesControl control)) return;

            control.OnPropertyChanged(nameof(IsGeneralSelected));
            control.OnPropertyChanged(nameof(IsAlbedoSelected));
            control.OnPropertyChanged(nameof(IsHeightSelected));
            control.OnPropertyChanged(nameof(IsNormalSelected));
            control.OnPropertyChanged(nameof(IsOcclusionSelected));
            control.OnPropertyChanged(nameof(IsSpecularSelected));
            control.OnPropertyChanged(nameof(IsSmoothSelected));
            control.OnPropertyChanged(nameof(IsRoughSelected));
            control.OnPropertyChanged(nameof(IsMetalSelected));
            control.OnPropertyChanged(nameof(IsPorositySelected));
            control.OnPropertyChanged(nameof(IsSssSelected));
            control.OnPropertyChanged(nameof(IsEmissiveSelected));
        }

        public static readonly DependencyProperty TextureProperty = DependencyProperty
            .Register("Texture", typeof(PbrProperties), typeof(PbrPropertiesControl), new PropertyMetadata(OnTexturePropertyChanged));

        public static readonly DependencyProperty SelectedProperty = DependencyProperty
            .Register("Selected", typeof(string), typeof(PbrPropertiesControl), new PropertyMetadata(OnSelectedPropertyChanged));

        public class TextureItem
        {
            public string Name {get; set;}
            public string Key {get; set;}
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TextureItemList : List<PbrPropertiesControl.TextureItem>
    {
        public TextureItemList()
        {
            Add(new PbrPropertiesControl.TextureItem {Name = "General"});
            Add(new PbrPropertiesControl.TextureItem {Name = "Albedo", Key = TextureTags.Albedo});
            Add(new PbrPropertiesControl.TextureItem {Name = "Height", Key = TextureTags.Height});
            Add(new PbrPropertiesControl.TextureItem {Name = "Normal", Key = TextureTags.Normal});
            Add(new PbrPropertiesControl.TextureItem {Name = "Occlusion", Key = TextureTags.Occlusion});
            Add(new PbrPropertiesControl.TextureItem {Name = "Specular", Key = TextureTags.Specular});
            Add(new PbrPropertiesControl.TextureItem {Name = "Smoothness", Key = TextureTags.Smooth});
            Add(new PbrPropertiesControl.TextureItem {Name = "Roughness", Key = TextureTags.Rough});
            Add(new PbrPropertiesControl.TextureItem {Name = "Metal", Key = TextureTags.Metal});
            Add(new PbrPropertiesControl.TextureItem {Name = "Porosity", Key = TextureTags.Porosity});
            Add(new PbrPropertiesControl.TextureItem {Name = "SSS", Key = TextureTags.SubSurfaceScattering});
            Add(new PbrPropertiesControl.TextureItem {Name = "Emissive", Key = TextureTags.Emissive});
        }
    }
}
