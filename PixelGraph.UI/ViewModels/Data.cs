using System.Windows;
using System.Windows.Data;

namespace PixelGraph.UI.ViewModels
{
    public interface ISearchParameters
    {
        string SearchText {get;}
        bool ShowAllFiles {get;}
    }

    public class DataGridProperty : DependencyObject
    {
        public string Name {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public Binding Value {
            get => (Binding)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }


        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(DataGridProperty));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Binding), typeof(DataGridProperty));
    }
}
