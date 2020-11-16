using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class TextBoxEx
    {
        public object Placeholder {
            get => GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }


        public TextBoxEx()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(object), typeof(TextBoxEx));
    }
}
