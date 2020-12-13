using System.Windows;
using System.Windows.Media;

namespace PixelGraph.UI.Controls
{
    public partial class ComboBoxEx
    {
        public object Placeholder {
            get => GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public Brush PlaceholderForeground {
            get => (Brush)GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }


        public ComboBoxEx()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(object), typeof(ComboBoxEx));

        public static readonly DependencyProperty PlaceholderForegroundProperty = DependencyProperty.Register("PlaceholderForeground", typeof(Brush), typeof(ComboBoxEx));
    }
}
