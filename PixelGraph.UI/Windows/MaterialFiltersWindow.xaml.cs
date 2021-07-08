using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class MaterialFiltersWindow
    {
        public MaterialFiltersWindow()
        {
            InitializeComponent();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            // TODO

            DialogResult = true;
        }
    }
}
