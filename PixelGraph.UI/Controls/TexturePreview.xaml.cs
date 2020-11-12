using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PixelGraph.UI.Controls
{
    public partial class TexturePreview
    {
        private static readonly DiffuseMaterial defaultAlbedoMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Green));

        public DiffuseMaterial AlbedoMaterial {
            get => (DiffuseMaterial)GetValue(AlbedoMaterialProperty);
            set => SetValue(AlbedoMaterialProperty, value ?? defaultAlbedoMaterial);
        }


        public TexturePreview()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AlbedoMaterialProperty = DependencyProperty
            .Register("AlbedoMaterial", typeof(DiffuseMaterial), typeof(TexturePreview), new PropertyMetadata(defaultAlbedoMaterial));
    }
}
