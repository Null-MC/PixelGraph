using System.Windows;
using System.Windows.Media;

namespace PixelGraph.UI.Controls;

public partial class TextBoxEx
{
    public object Placeholder {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public Brush PlaceholderForeground {
        get => (Brush)GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }


    public TextBoxEx()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register("Placeholder", typeof(object), typeof(TextBoxEx));

    public static readonly DependencyProperty PlaceholderForegroundProperty =
        DependencyProperty.Register("PlaceholderForeground", typeof(Brush), typeof(TextBoxEx));
}