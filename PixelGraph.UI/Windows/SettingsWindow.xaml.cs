using System;
using System.Windows;
using System.Windows.Markup;

namespace PixelGraph.UI.Windows
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(IServiceProvider provider)
        {
            InitializeComponent();
        }
    }

    [ContentProperty(nameof(Content))]
    public class TestRow
    {
        public string Header {get; set;}
        public object Content {get; set;}
    }
}
