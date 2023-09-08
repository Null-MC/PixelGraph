using System.Windows;

namespace PixelGraph.UI.Controls;

public partial class CheckBoxEx
{
    public bool? Placeholder {
        get => (bool?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }


    public CheckBoxEx()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(bool?), typeof(CheckBoxEx));
}