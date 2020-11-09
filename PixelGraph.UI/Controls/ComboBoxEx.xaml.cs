using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class ComboBoxEx
    {
        public object ItemsSource {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string Placeholder {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public object SelectedValue {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }


        public ComboBoxEx()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(ComboBoxEx));
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(ComboBoxEx));
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(object), typeof(ComboBoxEx),
            new FrameworkPropertyMetadata {BindsTwoWayByDefault = true});
    }
}
