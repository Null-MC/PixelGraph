using PixelGraph.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixelGraph.UI.Controls
{
    public partial class LogListControl
    {
        public LogListVM Log {
            get => (LogListVM) GetValue(LogProperty);
            set => SetValue(LogProperty, value);
        }


        public LogListControl()
        {
            InitializeComponent();
        }

        public void ScrollToEnd()
        {
            GetScrollViewer()?.ScrollToBottom();
        }

        private ScrollViewer GetScrollViewer()
        {
            var listbox = (ListBox)VisualTreeHelper.GetChild(this, 0);
            var border = (Border)VisualTreeHelper.GetChild(listbox, 0);
            return (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
        }

        public static readonly DependencyProperty LogProperty = DependencyProperty
            .Register("Log", typeof(LogListVM), typeof(LogListControl));
    }
}
