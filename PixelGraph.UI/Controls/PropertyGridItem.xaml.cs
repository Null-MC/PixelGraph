using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class PropertyGridItem
    {
        public string Header {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }


        public PropertyGridItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty
            .Register("Header", typeof(string), typeof(PropertyGridItem));
    }
}
