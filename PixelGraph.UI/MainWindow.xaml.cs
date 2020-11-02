using System;
using System.Windows;

namespace PixelGraph.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileNewMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FileExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
