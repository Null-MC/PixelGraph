using System.Windows;
using System.Windows.Controls;

namespace PixelGraph.UI.ViewModels
{
    public class PackPropertyItem : ListViewItem
    {
        public string PropertyName {
            get => (string)GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        public object PropertyValue {
            get => GetValue(PropertyValueProperty);
            set => SetValue(PropertyValueProperty, value);
        }


        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(PackPropertyItem));
        public static readonly DependencyProperty PropertyValueProperty = DependencyProperty.Register("PropertyValue", typeof(string), typeof(PackPropertyItem));
    }
}
