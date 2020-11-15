using PixelGraph.Common;
using System;
using System.ComponentModel;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class PbrPropertiesControl
    {
        public event EventHandler DataChanged;
        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;

        public PbrProperties Texture {
            get => (PbrProperties)GetValue(TextureProperty);
            set => SetValue(TextureProperty, value);
        }


        public PbrPropertiesControl()
        {
            InitializeComponent();
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

        public static readonly DependencyProperty TextureProperty = DependencyProperty
            .Register("Texture", typeof(PbrProperties), typeof(PbrPropertiesControl), new PropertyMetadata(OnTexturePropertyChanged));
    }
}
